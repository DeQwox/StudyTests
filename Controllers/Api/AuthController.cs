using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudyTests.Models.DTO.Api;

namespace StudyTests.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class AuthController(UserManager<IdentityUser> userManager) : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager = userManager;

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginModel model)
    {
        var user = await _userManager.FindByNameAsync(model.Username);
        if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
        {
            return Ok(new { UserId = user.Id });
        }
        return Unauthorized("Invalid credentials");
    }
}