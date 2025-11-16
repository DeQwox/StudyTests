using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using StudyTests.Data;
using StudyTests.Models.Entities;
using System.Security.Claims;
using StudyTests.Models.DTO.Authorization;
using BCrypt;
using Microsoft.AspNetCore.Identity;
using Duende.IdentityServer;
using Microsoft.AspNetCore.Authorization;

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
            FullName = model.FullName,
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, model.Role);
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
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        await HttpContext.SignOutAsync(IdentityServerConstants.DefaultCookieAuthenticationScheme);
        await HttpContext.SignOutAsync("oidc");

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        if (!User.Identity!.IsAuthenticated)
            return RedirectToAction("Login");

        var user = await _userManager.GetUserAsync(User);
        return View(user);
    }

    public IActionResult AccessDenied() => View();

    [HttpGet]
    public IActionResult ExternalLogin(string provider = "oidc", string returnUrl = "/Profile")
    {
        var routeValues = new Dictionary<string, string?> { ["returnUrl"] = returnUrl };
        var redirectUrl = Url.Action("ExternalLoginCallback", "Account", routeValues);
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

        return Challenge(properties, provider);
    }

    [HttpGet]
    public IActionResult ExternalLoginCallback()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Profile");

        return RedirectToAction("Index", "Home");
    }





    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> DeleteAccount()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            return RedirectToAction("Logout");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View("Index");
    }
}