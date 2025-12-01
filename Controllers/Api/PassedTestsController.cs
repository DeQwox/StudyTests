using Microsoft.AspNetCore.Mvc;
using System.Linq;
using StudyTests.Models.DTO.Api;
using StudyTests.Services.Api;
using StudyTests.Data;
using Microsoft.EntityFrameworkCore;
using StudyTests.Services;

namespace StudyTests.Controllers.Api
{
    [ApiController]
    [Route("api/passed-tests")]
    public class PassedTestsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IPassedTestsService _service;
        private readonly ILogger<PassedTestsController> _logger;

        public PassedTestsController(ApplicationDbContext db, IPassedTestsService service, ILogger<PassedTestsController> logger)
        {
            _db = db;
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll([FromQuery] int? studentId, [FromQuery] int? testId)
            => Ok(await _service.GetAllAsync(studentId, testId));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<object>> GetById(int id)
        {
            var p = await _service.GetByIdAsync(id);
            return p == null ? NotFound() : Ok(p);
        }

        [HttpPost]
        public async Task<ActionResult<object>> Create([FromBody] PassedTestDto dto)
        {
            _logger.LogInformation("[API] Received request to create PassedTest. StudentId: {StudentId}, TestId: {TestId}, Score: {Score}, ContentType: {ContentType}",
                dto.StudentId, dto.TestId, dto.Score, Request.ContentType);

            // Log incoming headers to help diagnose mobile vs swagger differences (CORS / antiforgery / content-type)
            foreach (var header in Request.Headers)
            {
                _logger.LogInformation("[API] Header: {Key}={Value}", header.Key, string.Join(", ", header.Value.ToArray()));
            }

            // Always use the userId from the token for the record, ignoring 0 or null from DTO
            if (!TryGetTokenUser(out var userId, out var role) || !string.Equals(role, "Student", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("[API] Unauthorized attempt to create PassedTest. Role: {Role}", role);
                return Forbid();
            }

            // Ensure the student creates the test for themselves
            // If studentId is provided and NOT 0, it must match the token userId
            if (dto.StudentId.HasValue && dto.StudentId.Value != 0 && dto.StudentId.Value != userId)
            {
                 _logger.LogWarning("[API] Student {UserId} tried to create test for another student {StudentId}", userId, dto.StudentId);
                 return Forbid();
            }
            var finalDto = dto with { StudentId = userId };

            try
            {
                var serializedDto = System.Text.Json.JsonSerializer.Serialize(finalDto);
                _logger.LogInformation("[API] Parsed DTO Body: {Json}", serializedDto);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[API] Could not serialize DTO for logging");
            }

            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                _logger.LogWarning("[API] ModelState invalid when creating PassedTest: {Errors}", errors);
                return BadRequest(ModelState);
            }

            try
            {
                var created = await _service.CreateAsync(finalDto);

                if (created == null)
                {
                    _logger.LogWarning("[API] Failed to create PassedTest. Student or Test not found.");
                    return BadRequest(new { error = "Student or Test not found" });
                }

                _logger.LogInformation("[API] Successfully created PassedTest with ID: {Id}", created.Id);

                var response = new
                {
                    id = created.Id,
                    studentId = created.StudentId,
                    testId = created.TestId,
                    answers = created.Answers,
                    score = created.Score,
                    passedAt = created.PassedAt
                };

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[API] Exception while creating PassedTest");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] PassedTestDto dto)
            => await _service.UpdateAsync(id, dto) ? NoContent() : NotFound();

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();

        // GET /api/passed-tests/by-teacher (Teacher only, teacherId from token)
        [HttpGet("by-teacher")]
        public async Task<IActionResult> GetPassedTestsByTeacher()
        {
            if (!TryGetTokenUser(out var teacherId, out var role) || !string.Equals(role, "Teacher", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            // Get all PassedTests for tests created by this teacher
            var passedTests = await _db.PassedTests
                .Include(p => p.Test)
                .Include(p => p.Student)
                .Where(p => p.Test.TeacherID == teacherId)
                .Select(p => new
                {
                    passedTestId = p.Id,
                    testId = p.TestId,
                    testName = p.Test.Name,
                    studentId = p.StudentId,
                    studentName = p.Student != null ? p.Student.FullName : null,
                    score = p.Score,
                    passedAt = p.PassedAt,
                    answers = p.Answers
                })
                .ToListAsync();

            return Ok(passedTests);
        }

        // GET /api/passed-tests/by-student (Student only, studentId from token)
        [HttpGet("by-student")]
        public async Task<IActionResult> GetPassedTestsByStudent()
        {
            if (!TryGetTokenUser(out var studentId, out var role) || !string.Equals(role, "Student", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var passedTests = await _db.PassedTests
                .Include(p => p.Test)
                .Where(p => p.StudentId == studentId)
                .Select(p => new
                {
                    passedTestId = p.Id,
                    testId = p.TestId,
                    testName = p.Test.Name,
                    score = p.Score,
                    passedAt = p.PassedAt,
                    answers = p.Answers
                })
                .ToListAsync();

            return Ok(passedTests);
        }

        private bool TryGetTokenUser(out int userId, out string role)
        {
            userId = 0; role = string.Empty;
            string? token = null;
            if (Request.Headers.TryGetValue("Authorization", out var vals))
            {
                var v = vals.FirstOrDefault();
                if (!string.IsNullOrEmpty(v) && v.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    token = v.Substring(7).Trim();
            }
            if (token == null && Request.Headers.TryGetValue("X-Access-Token", out var v2)) token = v2.FirstOrDefault();
            if (token == null) return false;
            return SimpleTokenStore.TryValidate(token, out userId, out role);
        }
    }
}
