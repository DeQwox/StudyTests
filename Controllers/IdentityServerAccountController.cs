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

        var testUser = Config.Users.FirstOrDefault(u => u.Username == model.Username && u.Password == model.Password);
        if (testUser == null)
        {
            ModelState.AddModelError("", "Invalid username or password");
            return View(model);
        }

        var email = testUser.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

        var appUser = await _userManager.FindByEmailAsync(email!);
        if (appUser == null)
        {
            var name = testUser.Claims.FirstOrDefault(c => c.Type == "name")?.Value!;
            var phone = testUser.Claims.FirstOrDefault(c => c.Type == "phone")?.Value;
            appUser = new User { 
                UserName = testUser.Username,
                FullName = name,
                Email = email,
                PhoneNumber = phone,
                Password = testUser.Password
            };
            var createResult = await _userManager.CreateAsync(appUser);
            if (!createResult.Succeeded)
            {
                Console.WriteLine(string.Join("\n", createResult.Errors.Select(i => i.Description)));
                ModelState.AddModelError("", "Failed to create user");
                return View(model);
            }
        }

        if (!await _userManager.IsInRoleAsync(appUser, "Student"))
        {
            var roleResult = await _userManager.AddToRoleAsync(appUser, "Student");
            if (!roleResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to assign role");
                return View(model);
            }
        }

        await _signInManager.SignInAsync(appUser, isPersistent: false);

        var claims = new List<Claim>
        {
            new("sub", testUser.SubjectId),
            new("name", testUser.Username),
            new(ClaimTypes.Role, "Student")
        };

        claims.AddRange(testUser.Claims);

        var identity = new ClaimsIdentity(claims, IdentityServerConstants.DefaultCookieAuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(IdentityServerConstants.DefaultCookieAuthenticationScheme, principal);

        if (_interaction.IsValidReturnUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return Redirect("~/");
    }
}
