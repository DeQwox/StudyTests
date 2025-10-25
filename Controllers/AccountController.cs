using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using StudyTests.Data;
using StudyTests.Models.Entities;
using System.Security.Claims;
using StudyTests.Models.DTO.Authorization;
using BCrypt;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace StudyTests.Controllers;

public class AccountController(ApplicationDbContext context, UserManager<User> userManager, SignInManager<User> signInManager) : Controller
{
    private readonly ApplicationDbContext _context = context;
    private readonly UserManager<User> _userManager = userManager;
    private readonly SignInManager<User> _signInManager = signInManager;

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new User
        {
            UserName = model.Login,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Profile");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

        return View(model);
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _signInManager.PasswordSignInAsync(model.Login!, model.Password!, false, false);
        if (result.Succeeded)
            return RedirectToAction("Profile");

        ModelState.AddModelError("", "Invalid login or password");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        if (!User.Identity!.IsAuthenticated)
            return RedirectToAction("Login");

        var user = await _userManager.GetUserAsync(User);
        return View(user);
    }

    public IActionResult AccessDenied() => View();
}

