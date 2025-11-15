using Microsoft.AspNetCore.Mvc;
using Repositories;
using System.Diagnostics;
using OpenTelemetry.Trace;

namespace StudyTests.Controllers.Api;

[ApiController]
public class TestsController(ITestingRepository repo) : ControllerBase
{
    private readonly ITestingRepository _repo = repo;

    // Джерело активностей — один раз на контролер
    private static readonly ActivitySource ActivitySource = new("StudyTests.Api");

    // v1: мінімальний контракт
    [HttpGet("api/v1/tests/{id:int}")]
    public async Task<IActionResult> GetV1(int id)
    {
        using var activity = ActivitySource.StartActivity("GetTest.V1", ActivityKind.Internal);
        activity?.SetTag("test.version", "v1");
        activity?.SetTag("test.id", id);

        var test = await _repo.GetTestByIdAsync(id);

        if (test == null)
        {
            activity?.SetStatus(Status.Error.WithDescription("Test not found"));
            return NotFound();
        }

        activity?.AddEvent(new ActivityEvent("Test found in DB"));

        var result = new
        {
            id = test.Id,
            name = test.Name,
            description = test.Description
        };

        return Ok(result);
    }

    // v2: повний контракт + кастомні поля + довгий SPAN
    [HttpGet("api/v2/tests/{id:int}")]
    public async Task<IActionResult> GetV2(int id)
    {
        // Головний бізнес-SPAN
        using var mainActivity = ActivitySource.StartActivity("GetTest.V2", ActivityKind.Internal);
        mainActivity?.SetTag("test.version", "v2");
        mainActivity?.SetTag("test.id", id);
        mainActivity?.SetTag("custom.user.role", "student");         // кастомне поле
        mainActivity?.SetTag("custom.request.source", "web-ui");     // кастомне поле

        var test = await _repo.GetTestByIdAsync(id);
        if (test == null)
        {
            mainActivity?.SetStatus(Status.Error.WithDescription("Test not found"));
            return NotFound();
        }

        mainActivity?.AddEvent(new ActivityEvent("Test entity loaded"));

        // ──────────────────────────────────────────────────────────────
        // Довготривалий процес (вимога пункту 3b)
        // ──────────────────────────────────────────────────────────────
        using (var longSpan = ActivitySource.StartActivity("LongRunningProcess: LoadQuestionsAndTeacher", ActivityKind.Internal))
        {
            longSpan?.SetTag("operation.type", "data-enrichment");
            longSpan?.SetTag("test.id", id);
            longSpan?.AddEvent(new ActivityEvent("Початок збагачення даних"));

            // Симулюємо важку роботу
            await Task.Delay(12_000);

            longSpan?.AddEvent(new ActivityEvent(
                "50% завершено",
                DateTimeOffset.Now,
                new ActivityTagsCollection(new[]
                {
                    new KeyValuePair<string, object?>("progress.percent", 50)
                })
            ));

            await Task.Delay(8_000);

            longSpan?.SetTag("custom.records.processed", 2847);
            longSpan?.AddEvent(new ActivityEvent("Збагачення завершено"));
        }

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
            validUntil = test.ValidUntil,
            custom_processed_at = DateTime.UtcNow
        };

        mainActivity?.SetTag("response.question.count", questions.Count);
        mainActivity?.SetTag("response.total.score", totalMax);

        return Ok(result);
    }
}
