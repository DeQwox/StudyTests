using Microsoft.AspNetCore.Mvc;
using StudyTests.Models.DTO.Api;
using StudyTests.Services.Api;
using StudyTests.Data;
using Microsoft.AspNetCore.Identity;
using StudyTests.Models.Entities;
using Microsoft.EntityFrameworkCore;
using StudyTests.Services;

namespace StudyTests.Controllers.Api;

[ApiController]
[Route("api/tests")]

public class TestsCrudController : ControllerBase
{
    // PUT /api/tests/full/{id} (Teacher) - update test and questions
    [HttpPut("full/{id:int}")]
    public async Task<IActionResult> UpdateFull(int id, [FromBody] Models.DTO.Api.TestWithQuestionsCreateDto dto)
    {
        if (!TryGetTokenUser(out var userId, out var role) || !string.Equals(role, "Teacher", StringComparison.OrdinalIgnoreCase))
            return Forbid();

        var test = await _db.Tests.Include(t => t.Questions).FirstOrDefaultAsync(t => t.Id == id && t.TeacherID == userId);
        if (test == null)
            return NotFound(new { error = "Test not found or access denied" });

        // Update test fields
        test.Name = dto.Name;
        test.Description = dto.Description ?? string.Empty;
        test.Password = dto.Password ?? string.Empty;
        test.ValidUntil = dto.ValidUntil;

        // Remove old questions
        _db.Questions.RemoveRange(test.Questions);
        await _db.SaveChangesAsync();

        // Add new questions
        test.Questions = new List<Question>();
        foreach (var q in dto.Questions)
        {
            test.Questions.Add(new Question
            {
                Description = q.Description,
                Answers = q.Answers,
                CorrectAnswerIndex = q.CorrectAnswerIndex,
                Score = q.Score
            });
        }

        await _db.SaveChangesAsync();

        return NoContent();
    }
    // GET /api/tests/my (Teacher) - get all tests for current teacher
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<object>>> GetMyTests()
    {
        if (!TryGetTokenUser(out var userId, out var role) || !string.Equals(role, "Teacher", StringComparison.OrdinalIgnoreCase))
            return Forbid();

        var tests = await _db.Tests
            .Where(t => t.TeacherID == userId)
            .Select(t => new {
                id = t.Id,
                name = t.Name,
                description = t.Description,
                createdAt = t.CreatedAt,
                validUntil = t.ValidUntil,
                questionCount = t.Questions.Count
            })
            .ToListAsync();
        return Ok(tests);
    }
    private readonly ApplicationDbContext _db;
    private readonly ITestsCrudService _service;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<TestsCrudController> _logger;

    public TestsCrudController(ApplicationDbContext db, ITestsCrudService service, UserManager<User> userManager, ILogger<TestsCrudController> logger)
    {
        _db = db;
        _service = service;
        _userManager = userManager;
        _logger = logger;
    }

    // POST /api/tests/full (Teacher) - create test and questions in one request
    [HttpPost("full")]
    public async Task<ActionResult<object>> CreateFull([FromBody] Models.DTO.Api.TestWithQuestionsCreateDto dto)
    {
        if (!TryGetTokenUser(out var userId, out var role) || !string.Equals(role, "Teacher", StringComparison.OrdinalIgnoreCase))
            return Forbid();

        // Create Test entity
        var test = new Test
        {
            TeacherID = userId,
            Name = dto.Name,
            Description = dto.Description ?? string.Empty,
            Password = dto.Password ?? string.Empty,
            ValidUntil = dto.ValidUntil,
            CreatedAt = DateTime.UtcNow
        };

        // Add questions
        test.Questions = new List<Question>();
        foreach (var q in dto.Questions)
        {
            test.Questions.Add(new Question
            {
                Description = q.Description,
                Answers = q.Answers,
                CorrectAnswerIndex = q.CorrectAnswerIndex,
                Score = q.Score
            });
        }

        _db.Tests.Add(test);
        await _db.SaveChangesAsync();

        // Prepare response
        var response = new
        {
            id = test.Id,
            name = test.Name,
            description = test.Description,
            password = test.Password,
            validUntil = test.ValidUntil,
            questions = test.Questions.Select(q => new
            {
                id = q.Id,
                description = q.Description,
                answers = q.Answers,
                correctAnswerIndex = q.CorrectAnswerIndex,
                score = q.Score
            }).ToList()
        };
        return CreatedAtAction(nameof(GetById), new { id = test.Id }, response);
    }

    // GET /api/tests  (Teacher only) - return tests belonging to teacher
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAll()
    {
        // extract token from header
        if (!TryGetTokenUser(out var userId, out var role) || !string.Equals(role, "Teacher", StringComparison.OrdinalIgnoreCase))
            return Forbid();

        var tests = await _db.Tests.Where(t => t.TeacherID == userId)
            .Select(t => new { id = t.Id, name = t.Name, description = t.Description, passwordProtected = !string.IsNullOrEmpty(t.Password) })
            .ToListAsync();
        return Ok(tests);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TestReadDto>> GetById(int id)
    {
        var t = await _service.GetByIdAsync(id);
        return t == null ? NotFound() : Ok(t);
    }

    // POST /api/tests (Teacher) - create test using token's user as TeacherID
    [HttpPost]
    public async Task<ActionResult<TestReadDto>> Create([FromBody] CreateTestRequest dto)
    {
        if (!TryGetTokenUser(out var userId, out var role) || !string.Equals(role, "Teacher", StringComparison.OrdinalIgnoreCase))
            return Forbid();

        var testDto = new TestDto(userId, dto.Name, dto.Description ?? string.Empty, dto.Password ?? string.Empty, dto.ValidUntil);
        var created = await _service.CreateAsync(testDto);
        if (created == null)
            return BadRequest(new { error = "Teacher not found or user is not in 'Teacher' role" });
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // GET /api/tests/{id}/questions  (Teacher + Student)
    [HttpGet("{id:int}/questions")]
    public async Task<IActionResult> GetQuestions(int id)
    {
        if (!TryGetTokenUser(out var userId, out var role) || !(string.Equals(role, "Student", StringComparison.OrdinalIgnoreCase) || string.Equals(role, "Teacher", StringComparison.OrdinalIgnoreCase)))
            return Forbid();

        var questions = await _db.Questions.Where(q => q.TestId == id)
            .Select(q => new { id = q.Id, description = q.Description, answers = q.Answers, correctAnswerIndex = q.CorrectAnswerIndex, score = q.Score })
            .ToListAsync();
        return Ok(questions);
    }

    // GET /api/tests/by-password/full?password=...
    [HttpGet("by-password/full")]
    public async Task<ActionResult<TestWithStudentQuestionsDto>> GetTestWithQuestions([FromQuery] string password)
    {
        _logger.LogInformation("[API] Searching for test with password: '{Password}'", password);

        // Allow anonymous access if password is correct
        // if (!TryGetTokenUser(out var userId, out var role)) ...

        var tests = await _db.Tests.Where(t => t.Password == password).ToListAsync();
        _logger.LogInformation("[API] Found {Count} tests with password '{Password}'", tests.Count, password);

        if (tests.Count == 0) return NotFound(new { error = "Test not found" });

        // Take the first matching test
        var test = tests.First();
        _logger.LogInformation("[API] Selected Test ID: {TestId}", test.Id);

        var questions = await _db.Questions.Where(q => q.TestId == test.Id)
            .OrderBy(q => q.Id)
            .Select(q => new QuestionStudentDto(q.Id, q.TestId, q.Description, q.Answers, q.Score))
            .ToListAsync();
            
        _logger.LogInformation("[API] Found {Count} questions for Test ID {TestId}", questions.Count, test.Id);

        return Ok(new TestWithStudentQuestionsDto(test.Id, test.TeacherID, test.Name, test.Description, test.CreatedAt, test.ValidUntil, questions));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TestDto dto)
        => await _service.UpdateAsync(id, dto) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => await _service.DeleteAsync(id) ? NoContent() : NotFound();

    // GET /api/tests/available (Student) - return tests available to students
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable()
    {
        if (!TryGetTokenUser(out var userId, out var role) || !string.Equals(role, "Student", StringComparison.OrdinalIgnoreCase))
            return Forbid();

        var now = DateTime.UtcNow;
        var tests = await _db.Tests
            .Where(t => t.ValidUntil == null || t.ValidUntil > now)
            .Select(t => new { id = t.Id, name = t.Name, hasPassword = !string.IsNullOrEmpty(t.Password) })
            .ToListAsync();
        return Ok(tests);
    }

    // Token helper
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

public record CreateTestRequest(string Name, string? Description, string? Password, DateTime? ValidUntil);

