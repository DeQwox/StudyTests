using Microsoft.AspNetCore.Mvc;
using StudyTests.Models.DTO;
using Repositories;
using StudyTests.Models.Entities;
using Microsoft.AspNetCore.Identity;
using StudyTests.Models.DTO.Tests;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace StudyTests.Controllers;

public class TestsController(ITestingRepository repo, UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager) : Controller
{
    private readonly ITestingRepository _repo = repo;

    private readonly UserManager<User> _userManager = userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager = roleManager;

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var tests = await _repo.GetAllTestsAsync();
        
        if (User?.Identity != null && User.Identity.IsAuthenticated)
        {
            var user = await _userManager.GetUserAsync(User);
            var passedTests = await _repo.GePassedTestsByStudentAsync(user!.Id);

            var model = tests.Select(test => new TestViewModel
            {
                Test = test,
                Passed = passedTests.FirstOrDefault(p => p.TestId == test.Id)
            }).ToList();

            return View(model);
        }

        var guestModel = tests.Select(t => new TestViewModel
        {
            Test = t,
            Passed = null
        }).ToList();

        return View(guestModel);
    }

    [AllowAnonymous]    
    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        
        var test = await _repo.GetTestByIdAsync(id);

        if (test == null) return NotFound();

        
        if (User?.Identity != null && User.Identity.IsAuthenticated)
        {
            var passed = (await _repo.GePassedTestsByStudentAsync(user!.Id))
                .FirstOrDefault(p => p.TestId == id);
            
            ViewData["Passed"] = passed;
        }
        
        return View(test);
    }

    [HttpGet]
    [Route("Create")]
    [Authorize(Roles = "Teacher")]
    public IActionResult Create()
    {
        return View(new TestCreateViewModel());
    }

    [HttpPost]
    [Route("AddTest")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> AddTest(TestCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Create", model);
        }

        if (model != null)
        {
            var user = await _userManager.GetUserAsync(User);
            model.TeacherId = user!.Id;
            Console.WriteLine(model.QuestionsJson);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            if (model.QuestionsJson != null)
            {
                List<Question>? questions = JsonSerializer.Deserialize<List<Question>>(model.QuestionsJson, options);
                if (questions != null)
                    model.Questions = questions;
            }

            await _repo.AddTestAsync(model);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Delete(int id)
    {
        if (! await _repo.RemoveTestAsync(id)) return NotFound();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("tests/old/{id:int}")]
    public IActionResult Old(int id)
    {
        ViewBag.TestId = id;
        return View("OldDetails");
    }

    [HttpGet("tests/new/{id:int}")]
    public IActionResult New(int id)
    {
        ViewBag.TestId = id;
        return View("NewDetails");
    }
}
