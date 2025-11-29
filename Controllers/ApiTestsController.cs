using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using StudyTests.Models.Entities;
using StudyTests.Models.DTO.Api1;
using Repositories;

namespace StudyTests.Controllers.Api
{
    [Route("api/tests")] // The URL Angular will call
    [ApiController]
    public class ApiTestsController : ControllerBase
    {
        private readonly ITestingRepository _repo;
        private readonly UserManager<User> _userManager;

        public ApiTestsController(ITestingRepository repo, UserManager<User> userManager)
        {
            _repo = repo;
            _userManager = userManager;
        }

        // GET: api/tests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestSummaryDto>>> GetTests()
        {
            var tests = await _repo.GetAllTestsAsync();
            var userId = _userManager.GetUserId(User);
            var passedTests = userId != null 
                ? await _repo.GePassedTestsByStudentAsync(int.Parse(userId)) 
                : new List<PassedTest>();

            var result = tests.Select(t => {
                var passed = passedTests.FirstOrDefault(p => p.TestId == t.Id);
                return new TestSummaryDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    TeacherName = t.Teacher?.FullName ?? "Unknown",
                    TeacherId = t.TeacherID.ToString(),
                    MaxScore = t.Questions?.Sum(q => q.Score) ?? 0,
                    Passed = passed == null ? null : new PassedInfo { Score = passed.Score, PassedAt = passed.PassedAt }
                };
            });

            return Ok(result);
        }

        // GET: api/tests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TestDetailDto>> GetTest(int id)
        {
            var test = await _repo.GetTestByIdAsync(id);
            if (test == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var passed = userId != null 
                ? (await _repo.GePassedTestsByStudentAsync(int.Parse(userId))).FirstOrDefault(p => p.TestId == id)
                : null;

            var dto = new TestDetailDto
            {
                Id = test.Id,
                Name = test.Name,
                Description = test.Description,
                TeacherName = test.Teacher?.FullName ?? "Unknown",
                TeacherId = test.TeacherID.ToString(),
                MaxScore = test.Questions?.Sum(q => q.Score) ?? 0,
                Passed = passed == null ? null : new PassedInfo { Score = passed.Score, PassedAt = passed.PassedAt },
                QuestionCount = test.Questions?.Count ?? 0,
                CreatedAt = test.CreatedAt,
                ValidUntil = test.ValidUntil,
                IsOwner = userId != null && test.TeacherID.ToString() == userId
            };

            return Ok(dto);
        }

        // POST: api/tests
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult> CreateTest([FromBody] CreateTestDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Map DTO to your Repository Model (TestCreateViewModel or Entity)
            var model = new Models.DTO.Tests.TestCreateViewModel
            {
                Name = dto.Name,
                Description = dto.Description,
                Password = dto.Password,
                ValidUntil = dto.ValidUntil,
                TeacherId = user.Id,
                Questions = dto.Questions.Select(q => new Question
                {
                    Description = q.Description,
                    Score = q.Score,
                    CorrectAnswerIndex = q.CorrectAnswerIndex,
                    Answers = q.Answers
                }).ToList()
            };

            // Note: We bypass the "QuestionsJson" logic here because 
            // Angular sends a real object, not a stringified form field.
            await _repo.AddTestAsync(model);

            return Ok(new { message = "Test created successfully" });
        }

        // DELETE: api/tests/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteTest(int id)
        {
            var test = await _repo.GetTestByIdAsync(id);
            if (test == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            
            // Ensure only the owner can delete
            if (test.TeacherID != user?.Id) return Forbid();

            if (!await _repo.RemoveTestAsync(id)) return NotFound();

            return Ok();
        }
    }
}