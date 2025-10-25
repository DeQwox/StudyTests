using Microsoft.AspNetCore.Mvc;
using Models.DTO.Authorization;
using StudyTests.Repositories;

namespace StudyTests.Controllers;

public class AccountController(IAccountRepository accountRepository) : Controller
{
    private readonly IAccountRepository _accountRepository = accountRepository;

    [HttpGet]
    [Route("register")]
    public IActionResult Register()
    {
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("RegistrationUser")]
    public async Task<IActionResult> RegistrationUser(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _accountRepository.AddUserAsync(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Не вдалося створити користувача: " + ex.Message);
            return View(model);
        }

        return RedirectToAction("Login");
    }
    [HttpGet]
    [Route("login")]
    public IActionResult Login()
    {
        // Implementation for retrieving a student by ID
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("LoginUser")]
    public async Task<IActionResult> LoginUser(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _accountRepository.LoginUserAsync(model);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Невірне ім'я користувача або пароль.");
            return View(model);
        }

        // Here you would typically sign in the user using ASP.NET Core Identity or similar
        // For example:
        // await HttpContext.SignInAsync(...);

        return RedirectToAction("Index", "Home");
    }
    
}