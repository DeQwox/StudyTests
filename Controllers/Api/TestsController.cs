using Microsoft.AspNetCore.Mvc;
using Repositories;

namespace StudyTests.Controllers.Api;

[ApiController]
public class TestsController(ITestingRepository repo) : ControllerBase
{
    private readonly ITestingRepository _repo = repo;

    // v1: minimal contract
    // GET /api/v1/tests/{id}
    [HttpGet("api/v1/tests/{id:int}")]
    public async Task<IActionResult> GetV1(int id)
    {
        var test = await _repo.GetTestByIdAsync(id);
        if (test == null) return NotFound();

        var result = new
        {
            id = test.Id,
            name = test.Name,
            description = test.Description
        };
        return Ok(result);
    }

    // v2: superset of v1
    // GET /api/v2/tests/{id}
    [HttpGet("api/v2/tests/{id:int}")]
    public async Task<IActionResult> GetV2(int id)
    {
        var test = await _repo.GetTestByIdAsync(id);
        if (test == null) return NotFound();

        var questions = _repo.GetTestQuestionList(id).ToList();
        var totalMax = questions.Sum(q => q.Score);
        var teacher = await _repo.GetTeacherByIdAsync(test.TeacherID);

        var result = new
        {
            id = test.Id,
            name = test.Name,
            description = test.Description,
            teacherId = test.TeacherID,
            teacherName = teacher?.FullName ?? string.Empty,
            questionCount = questions.Count,
            totalMaxScore = totalMax,
            createdAt = test.CreatedAt,
            validUntil = test.ValidUntil
        };
        return Ok(result);
    }
}
