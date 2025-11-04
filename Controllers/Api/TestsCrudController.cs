using Microsoft.AspNetCore.Mvc;
using StudyTests.Models.DTO.Api;
using StudyTests.Services.Api;

namespace StudyTests.Controllers.Api;

[ApiController]
[Route("api/tests")]
public class TestsCrudController(ITestsCrudService service) : ControllerBase
{
    private readonly ITestsCrudService _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TestReadDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TestReadDto>> GetById(int id)
    {
        var t = await _service.GetByIdAsync(id);
        return t == null ? NotFound() : Ok(t);
    }

    [HttpPost]
    public async Task<ActionResult<TestReadDto>> Create([FromBody] TestDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TestDto dto)
        => await _service.UpdateAsync(id, dto) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => await _service.DeleteAsync(id) ? NoContent() : NotFound();
}
