using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using StudyTests.Models.Entities;
using StudyTests.Services;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace StudyTests.Controllers.Api;

[ApiController]
[Route("api/auth")]
public class AuthController(UserManager<User> userManager) : ControllerBase
{
    private readonly UserManager<User> _userManager = userManager;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.Login) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { error = "login and password required" });

        var user = await _userManager.FindByNameAsync(req.Login) ?? await _userManager.FindByEmailAsync(req.Login);
        if (user == null) return Unauthorized();
        var ok = await _userManager.CheckPasswordAsync(user, req.Password);
        if (!ok) return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";

        var token = SimpleTokenStore.Generate(user.Id, role);

        return Ok(new { accessToken = token, role });
    }

    // POST: api/auth/register
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] StudyTests.Models.DTO.Authorization.RegisterApiDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Login) || string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { error = "login, password, and email required" });

        var user = new User
        {
            UserName = dto.Login,
            Login = dto.Login,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            FullName = dto.FullName ?? string.Empty
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new { success = false, errors = result.Errors.Select(e => e.Description).ToList() });
        }

        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            var addRole = await _userManager.AddToRoleAsync(user, dto.Role);
            if (!addRole.Succeeded)
            {
                return BadRequest(new { success = false, errors = addRole.Errors.Select(e => e.Description).ToList() });
            }
        }

        return Ok(new { success = true, userId = user.Id });
    }
}

public record LoginRequest(string Login, string Password);
