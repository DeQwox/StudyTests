using Microsoft.AspNetCore.Mvc;
using StudyTests.Models.DTO.Api;
using StudyTests.Services.Api;

namespace StudyTests.Controllers.Api;

[ApiController]
[Route("api/questions")]
public class QuestionsController(IQuestionsService service) : ControllerBase
{
    private readonly IQuestionsService _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAll([FromQuery] int? testId)
        => Ok(await _service.GetAllAsync(testId));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> GetById(int id)
    {
        var q = await _service.GetByIdAsync(id);
        return q == null ? NotFound() : Ok(q);
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] QuestionDto dto)
    {
        var created = await _service.CreateAsync(dto);
        var idProp = created.GetType().GetProperty("Id")?.GetValue(created);
        return CreatedAtAction(nameof(GetById), new { id = idProp }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] QuestionDto dto)
        => await _service.UpdateAsync(id, dto) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => await _service.DeleteAsync(id) ? NoContent() : NotFound();
}
