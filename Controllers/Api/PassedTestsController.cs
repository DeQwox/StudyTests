using Microsoft.AspNetCore.Mvc;
using StudyTests.Models.DTO.Api;
using StudyTests.Services.Api;

namespace StudyTests.Controllers.Api;

[ApiController]
[Route("api/passedtests")]
public class PassedTestsController(IPassedTestsService service) : ControllerBase
{
    private readonly IPassedTestsService _service = service;

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
        var created = await _service.CreateAsync(dto);
        if (created == null) return BadRequest(new { error = "Student or Test not found" });
        var idProp = created.GetType().GetProperty("Id")?.GetValue(created);
        return CreatedAtAction(nameof(GetById), new { id = idProp }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] PassedTestDto dto)
        => await _service.UpdateAsync(id, dto) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => await _service.DeleteAsync(id) ? NoContent() : NotFound();
}
