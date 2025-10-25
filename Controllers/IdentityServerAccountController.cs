using Duende.IdentityServer;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using StudyTests.Models.Entities;
using Microsoft.AspNetCore.Identity;
using StudyTests.Models.DTO.Authorization;
using StudyTests.Data.IdentityServer;

namespace StudyTests.Controllers;

public class IdentityServerAccountController : Controller
{
    private readonly IIdentityServerInteractionService _interaction;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public IdentityServerAccountController(
        IIdentityServerInteractionService interaction,
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        _interaction = interaction;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Login(string returnUrl)
    {
        return View(new IdentityServerLoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(IdentityServerLoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = Config.Users.FirstOrDefault(u => u.Username == model.Username && u.Password == model.Password);
        if (user == null)
        {
            ModelState.AddModelError("", "Invalid username or password");
            return View(model);
        }

        var appUser = await _userManager.FindByNameAsync(user.Username);
        if (appUser == null)
        {
            appUser = new User { UserName = user.Username };
            await _userManager.CreateAsync(appUser);
        }

        await _signInManager.SignInAsync(appUser, isPersistent: false);

        var claims = new[]
        {
            new Claim("sub", user.SubjectId),
            new Claim("name", user.Username)
        };
        var identity = new ClaimsIdentity(claims, IdentityServerConstants.DefaultCookieAuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(IdentityServerConstants.DefaultCookieAuthenticationScheme, principal);

        if (_interaction.IsValidReturnUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return Redirect("~/");
    }
}
