using Microsoft.AspNetCore.Mvc;
using StudyTests.Models.DTO;
using Repositories;
using StudyTests.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace StudyTests.Controllers;

public class TestsController(ITestingRepository repo, UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager) : Controller
{
    private readonly ITestingRepository _repo = repo;

    private readonly UserManager<User> _userManager = userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager = roleManager;


    public async Task<IActionResult> Index()
    {
        var tests = await _repo.GetAllTestsAsync();
        Console.WriteLine(tests.Count());
        Console.WriteLine("a");
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();

        return View(tests);
    }


    [HttpGet]
    [Route("Create")]
    public async Task<IActionResult> Create()
    {
        var vm = new TestCreateViewModel();
        
        var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
        ViewBag.Teachers = teachers;
        return View(vm);
    }

    [HttpPost]
    [Route("AddTest")]
    public async Task<IActionResult> AddTest(TestCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Create", model);
        }

        if (model != null)
        {
            await _repo.AddTestAsync(model);
        }

        return RedirectToAction("Index");
    }
}
