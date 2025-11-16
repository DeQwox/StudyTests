using Microsoft.AspNetCore.Mvc;
using StudyTests.Services;
using StudyTests.Models.Entities;
using StudyTests.Data;
using Repositories;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace StudyTests.Controllers;

[Authorize]
public class TestingController(ITestingRepository testingRepository, UserManager<User> userManager) : Controller
{
    private readonly ITestingRepository _testingRepository = testingRepository;
    private readonly UserManager<User> _userManager = userManager;

    [HttpGet]
    public async Task<IActionResult> Start(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        int studentId = user.Id;

        Console.WriteLine($"Student id: {studentId}");

        var testing = new Testing(id, studentId, _testingRepository);
        HttpContext.Session.SetObject("testing", testing);

        return RedirectToAction("Question");
    }

    [HttpGet]
    public IActionResult Question()
    {
        var testing = HttpContext.Session.GetObject<Testing>("testing");
        if (testing == null) return RedirectToAction("Index", "Tests");
        testing.RestoreDependencies(_testingRepository);

        if (testing.GetQuestionsCount() == 0)
            return RedirectToAction("Finish");

        if (testing.Current >= testing.GetQuestionsCount())
            return RedirectToAction("Finish");

        var question = testing.GetQuestion();
        HttpContext.Session.SetObject("testing", testing);

        return View(question);
    }

    [HttpPost]
    public IActionResult Answer(int selectedAnswer)
    {
        var testing = HttpContext.Session.GetObject<Testing>("testing");
        if (testing == null) return RedirectToAction("Index", "Tests");
        testing.RestoreDependencies(_testingRepository);

        testing.Answer(selectedAnswer);
        HttpContext.Session.SetObject("testing", testing);

        if (testing.Current >= testing.GetQuestionsCount())
            return RedirectToAction("Finish");

        return RedirectToAction("Question");
    }

    [HttpGet]
    public async Task<IActionResult> Finish()
    {
        var testing = HttpContext.Session.GetObject<Testing>("testing");
        if (testing == null) return RedirectToAction("Index", "Tests");
        testing.RestoreDependencies(_testingRepository);

        var passed = await testing.GetResult();
        await _testingRepository.AddPassedTestAsync(passed);

        HttpContext.Session.Remove("testing");

        return View("Result", passed);
    }

    [HttpGet]
    public async Task<IActionResult> Result(int id)
    {
        var passedTest = await _testingRepository.GePassedTestByIdAsync(id);

        if (passedTest == null) return NotFound();
            
        return View(passedTest);
    }
}
