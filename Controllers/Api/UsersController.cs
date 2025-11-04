using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyTests.Models.DTO.Api;
using StudyTests.Models.Entities;

namespace StudyTests.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class UsersController(
    UserManager<User> userManager,
    RoleManager<IdentityRole<int>> roleManager
) : ControllerBase
{
    // GET: api/users?role=Teacher
    [HttpGet]
    [Authorize(Roles = "Teacher,Student")] // adjust as needed
    public async Task<ActionResult<IEnumerable<UserReadDto>>> GetAll([FromQuery] string? role)
    {
        List<User> users;
        if (!string.IsNullOrWhiteSpace(role))
        {
            var inRole = await userManager.GetUsersInRoleAsync(role);
            users = [.. inRole];
        }
        else
        {
            users = await userManager.Users.AsNoTracking().ToListAsync();
        }

        var result = new List<UserReadDto>(users.Count);
        foreach (var u in users)
        {
            var roles = await userManager.GetRolesAsync(u);
            result.Add(ToDto(u, roles));
        }
        return Ok(result);
    }

    // GET: api/users/5
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Teacher,Student")]
    public async Task<ActionResult<UserReadDto>> GetById(int id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();
        var roles = await userManager.GetRolesAsync(user);
        return Ok(ToDto(user, roles));
    }

    // POST: api/users
    [HttpPost]
    [Authorize(Roles = "Teacher")] // only teachers/admins create users
    public async Task<ActionResult<UserReadDto>> Create([FromBody] CreateUserDto dto)
    {
        var user = new User
        {
            UserName = dto.Login,
            Login = dto.Login,
            FullName = dto.FullName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber
        };

        var createResult = await userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["errors"] = createResult.Errors.Select(e => e.Description).ToArray()
            }));
        }

        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            var roleName = dto.Role.Trim();
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var roleCreate = await roleManager.CreateAsync(new IdentityRole<int> { Name = roleName, NormalizedName = roleName.ToUpper() });
                if (!roleCreate.Succeeded)
                {
                    return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
                    {
                        ["errors"] = roleCreate.Errors.Select(e => e.Description).ToArray()
                    }));
                }
            }
            var addRole = await userManager.AddToRoleAsync(user, roleName);
            if (!addRole.Succeeded)
            {
                return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    ["errors"] = addRole.Errors.Select(e => e.Description).ToArray()
                }));
            }
        }

        var roles = await userManager.GetRolesAsync(user);
        var dtoOut = ToDto(user, roles);
        return CreatedAtAction(nameof(GetById), new { id = dtoOut.Id }, dtoOut);
    }

    // PUT: api/users/5
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();

        user.UserName = dto.Login;
        user.Login = dto.Login;
        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.PhoneNumber = dto.PhoneNumber;

        var update = await userManager.UpdateAsync(user);
        if (!update.Succeeded)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["errors"] = update.Errors.Select(e => e.Description).ToArray()
            }));
        }

        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            var newRole = dto.Role.Trim();
            if (!await roleManager.RoleExistsAsync(newRole))
            {
                var roleCreate = await roleManager.CreateAsync(new IdentityRole<int> { Name = newRole, NormalizedName = newRole.ToUpper() });
                if (!roleCreate.Succeeded)
                {
                    return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
                    {
                        ["errors"] = roleCreate.Errors.Select(e => e.Description).ToArray()
                    }));
                }
            }

            var currentRoles = await userManager.GetRolesAsync(user);
            var toRemove = currentRoles.Where(r => !string.Equals(r, newRole, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (toRemove.Length > 0)
            {
                var removed = await userManager.RemoveFromRolesAsync(user, toRemove);
                if (!removed.Succeeded)
                {
                    return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
                    {
                        ["errors"] = removed.Errors.Select(e => e.Description).ToArray()
                    }));
                }
            }
            if (!currentRoles.Contains(newRole, StringComparer.OrdinalIgnoreCase))
            {
                var added = await userManager.AddToRoleAsync(user, newRole);
                if (!added.Succeeded)
                {
                    return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
                    {
                        ["errors"] = added.Errors.Select(e => e.Description).ToArray()
                    }));
                }
            }
        }

        return NoContent();
    }

    // DELETE: api/users/5
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();
        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["errors"] = result.Errors.Select(e => e.Description).ToArray()
            }));
        }
        return NoContent();
    }

    private static UserReadDto ToDto(User user, IEnumerable<string> roles)
        => new(user.Id, user.Login, user.FullName, user.Email, user.PhoneNumber, roles.ToArray());
}
