using Microsoft.AspNetCore.Mvc;
using StudyTests.Models.DTO;
using Repositories;
using StudyTests.Models.Entities;

namespace StudyTests.Controllers;

public class TestsController(ITestingRepository repo) : Controller
{
    private readonly ITestingRepository _repo = repo;

    [HttpGet]
    [Route("CreateTest")]
    public async Task<IActionResult> CreateTest()
    {
        var vm = new TestCreateViewModel();
        // optionally pass teachers to view via ViewBag
        var teachers = await _repo.GetAllTeachersAsync();
        ViewBag.Teachers = teachers;
        return View(vm);
    }

    [HttpPost]
    [Route("AddTest")]
    public async Task<IActionResult> AddTest(TestCreateViewModel model)
    {
        if (model != null)
        {
            await _repo.AddTestAsync(model);

        }
        return RedirectToAction("Index", "Home");
    }
}
