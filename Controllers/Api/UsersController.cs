using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyTests.Models.DTO.Api;
using StudyTests.Models.Entities;

namespace StudyTests.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class UsersController(
    UserManager<User> userManager,
    RoleManager<IdentityRole<int>> roleManager,
    ILogger<UsersController> logger
) : ControllerBase
{
    // GET: api/users?role=Teacher
    [HttpGet(Name = "GetAllUsers")]
    public async Task<ActionResult<IEnumerable<UserReadDto>>> GetAll([FromQuery] string? role)
    {
        logger.LogInformation("GetAll called with role='{role}'", role);
        List<User> users;
        if (!string.IsNullOrWhiteSpace(role))
        {
            var inRole = await userManager.GetUsersInRoleAsync(role);
            users = [.. inRole];
            logger.LogInformation("Found {count} users in role {role}", users.Count, role);
        }
        else
        {
            users = await userManager.Users.AsNoTracking().ToListAsync();
            logger.LogInformation("Found {count} total users", users.Count);
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
    [HttpGet("{id:int}", Name = "GetUserById")]
    public async Task<ActionResult<UserReadDto>> GetById(int id)
    {
        logger.LogInformation("GetById called for id={id}", id);
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            logger.LogWarning("User id={id} not found", id);
            return NotFound();
        }
        var roles = await userManager.GetRolesAsync(user);
        logger.LogInformation("Returning user id={id} with {roleCount} roles", id, roles.Count);
        return Ok(ToDto(user, roles));
    }

    // POST: api/users
    [HttpPost(Name = "CreateUser")]
    public async Task<ActionResult<UserReadDto>> Create([FromBody] CreateUserDto dto)
    {
        logger.LogInformation("Create called for Login={login}, Email={email}", dto.Login, dto.Email);

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
            logger.LogWarning("Create failed for Login={login}: {errors}", dto.Login, string.Join(";", createResult.Errors.Select(e => e.Description)));
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
                    logger.LogError("Failed to create role {role}: {errors}", roleName, string.Join(";", roleCreate.Errors.Select(e => e.Description)));
                    return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
                    {
                        ["errors"] = roleCreate.Errors.Select(e => e.Description).ToArray()
                    }));
                }
            }
            var addRole = await userManager.AddToRoleAsync(user, roleName);
            if (!addRole.Succeeded)
            {
                logger.LogWarning("Failed to add role {role} to user {login}: {errors}", roleName, dto.Login, string.Join(";", addRole.Errors.Select(e => e.Description)));
                return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    ["errors"] = addRole.Errors.Select(e => e.Description).ToArray()
                }));
            }
        }

        var roles = await userManager.GetRolesAsync(user);
        var dtoOut = ToDto(user, roles);
        logger.LogInformation("User created id={id}, login={login}", dtoOut.Id, dtoOut.Login);
        return CreatedAtRoute("GetUserById", new { id = dtoOut.Id }, dtoOut);
    }

    // PUT: api/users/5
    [HttpPut("{id:int}", Name = "UpdateUser")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        logger.LogInformation("Update called for id={id}, login={login}", id, dto.Login);
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            logger.LogWarning("Update failed: user id={id} not found", id);
            return NotFound();
        }

        user.UserName = dto.Login;
        user.Login = dto.Login;
        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.PhoneNumber = dto.PhoneNumber;

        var update = await userManager.UpdateAsync(user);
        if (!update.Succeeded)
        {
            logger.LogWarning("Update failed for id={id}: {errors}", id, string.Join(";", update.Errors.Select(e => e.Description)));
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["errors"] = update.Errors.Select(e => e.Description).ToArray()
            }));
        }

        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            var newRole = dto.Role.Trim();
            // Ensure role exists
            if (!await roleManager.RoleExistsAsync(newRole))
            {
                var roleCreate = await roleManager.CreateAsync(new IdentityRole<int> { Name = newRole, NormalizedName = newRole.ToUpper() });
                if (!roleCreate.Succeeded)
                {
                    logger.LogError("Failed to create role {role}: {errors}", newRole, string.Join(";", roleCreate.Errors.Select(e => e.Description)));
                    return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
                    {
                        ["errors"] = roleCreate.Errors.Select(e => e.Description).ToArray()
                    }));
                }
            }

            var currentRoles = (await userManager.GetRolesAsync(user)).ToArray();

            // Add the new role first (if not already in it), then remove other roles.
            if (!currentRoles.Contains(newRole, StringComparer.OrdinalIgnoreCase))
            {
                var added = await userManager.AddToRoleAsync(user, newRole);
                if (!added.Succeeded)
                {
                    logger.LogWarning("Failed add new role {role} for id={id}: {errors}", newRole, id, string.Join(";", added.Errors.Select(e => e.Description)));
                    return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
                    {
                        ["errors"] = added.Errors.Select(e => e.Description).ToArray()
                    }));
                }
            }

            var toRemove = currentRoles.Where(r => !string.Equals(r, newRole, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (toRemove.Length > 0)
            {
                var removed = await userManager.RemoveFromRolesAsync(user, toRemove);
                if (!removed.Succeeded)
                {
                    logger.LogWarning("Failed remove old roles for id={id}: {errors}", id, string.Join(";", removed.Errors.Select(e => e.Description)));
                    return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
                    {
                        ["errors"] = removed.Errors.Select(e => e.Description).ToArray()
                    }));
                }
            }
        }

        logger.LogInformation("Update successful for id={id}", id);
        return NoContent();
    }

    // DELETE: api/users/5
    [HttpDelete("{id:int}", Name = "DeleteUser")]
    public async Task<IActionResult> Delete(int id)
    {
        logger.LogInformation("Delete called for id={id}", id);
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            logger.LogWarning("Delete: user id={id} not found", id);
            return NotFound();
        }
        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            logger.LogWarning("Delete failed for id={id}: {errors}", id, string.Join(";", result.Errors.Select(e => e.Description)));
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["errors"] = result.Errors.Select(e => e.Description).ToArray()
            }));
        }
        logger.LogInformation("User id={id} deleted", id);
        return NoContent();
    }

    // PUT: api/users/{id}/setrole
    [HttpPut("{id:int}/setrole", Name = "SetUserRole")]
    public async Task<IActionResult> SetRole(int id, [FromBody] string? role)
    {
        logger.LogInformation("SetRole called for id={id} role={role}", id, role);
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            logger.LogWarning("SetRole: user id={id} not found", id);
            return NotFound();
        }

        var newRole = (role ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(newRole))
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["errors"] = new[] { "Role must be provided in the request body as a plain string." }
            }));
        }

        // ensure role exists
        if (!await roleManager.RoleExistsAsync(newRole))
        {
            var rc = await roleManager.CreateAsync(new IdentityRole<int> { Name = newRole, NormalizedName = newRole.ToUpper() });
            if (!rc.Succeeded)
            {
                logger.LogError("SetRole: failed to create role {role}: {errors}", newRole, string.Join(";", rc.Errors.Select(e => e.Description)));
                return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    ["errors"] = rc.Errors.Select(e => e.Description).ToArray()
                }));
            }
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        // remove all current roles first
        if (currentRoles.Any())
        {
            var rem = await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!rem.Succeeded)
            {
                logger.LogWarning("SetRole: failed to remove roles for id={id}: {errors}", id, string.Join(";", rem.Errors.Select(e => e.Description)));
                return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    ["errors"] = rem.Errors.Select(e => e.Description).ToArray()
                }));
            }
        }

        var add = await userManager.AddToRoleAsync(user, newRole);
        if (!add.Succeeded)
        {
            logger.LogWarning("SetRole: failed to add role {role} for id={id}: {errors}", newRole, id, string.Join(";", add.Errors.Select(e => e.Description)));
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["errors"] = add.Errors.Select(e => e.Description).ToArray()
            }));
        }

        logger.LogInformation("SetRole: role {role} set for user id={id}", newRole, id);
        return NoContent();
    }

    private static UserReadDto ToDto(User user, IEnumerable<string> roles)
        => new(user.Id, user.Login, user.FullName, user.Email, user.PhoneNumber, roles.ToArray());
}
