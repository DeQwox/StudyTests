using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyTests.Models.DTO.Api;
using StudyTests.Models.Entities;

namespace StudyTests.Services.Api;

public class UsersService(
    UserManager<User> userManager,
    RoleManager<IdentityRole<int>> roleManager
) : IUsersService
{
    public async Task<IEnumerable<UserReadDto>> GetAllAsync(string? role = null)
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
        return result;
    }

    public async Task<UserReadDto?> GetByIdAsync(int id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return null;
        var roles = await userManager.GetRolesAsync(user);
        return ToDto(user, roles);
    }

    public async Task<(bool ok, UserReadDto? result, IEnumerable<string>? errors)> CreateAsync(CreateUserDto dto)
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
            return (false, null, createResult.Errors.Select(e => e.Description));
        }

        // Ensure role exists, then add user to role
        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            var roleName = dto.Role.Trim();
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole<int> { Name = roleName, NormalizedName = roleName.ToUpper() });
                if (!roleResult.Succeeded)
                {
                    return (false, null, roleResult.Errors.Select(e => e.Description));
                }
            }

            var addRoleResult = await userManager.AddToRoleAsync(user, roleName);
            if (!addRoleResult.Succeeded)
            {
                return (false, null, addRoleResult.Errors.Select(e => e.Description));
            }
        }

        var roles = await userManager.GetRolesAsync(user);
        return (true, ToDto(user, roles), null);
    }

    public async Task<(bool ok, IEnumerable<string>? errors)> UpdateAsync(int id, UpdateUserDto dto)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return (false, new[] { $"User {id} not found" });
        }

        user.UserName = dto.Login;
        user.Login = dto.Login;
        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.PhoneNumber = dto.PhoneNumber;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return (false, updateResult.Errors.Select(e => e.Description));
        }

        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            var newRole = dto.Role.Trim();
            if (!await roleManager.RoleExistsAsync(newRole))
            {
                var roleCreate = await roleManager.CreateAsync(new IdentityRole<int> { Name = newRole, NormalizedName = newRole.ToUpper() });
                if (!roleCreate.Succeeded)
                {
                    return (false, roleCreate.Errors.Select(e => e.Description));
                }
            }

            var currentRoles = (await userManager.GetRolesAsync(user)).ToArray();

            // Add new role first if missing
            if (!currentRoles.Contains(newRole, StringComparer.OrdinalIgnoreCase))
            {
                var added = await userManager.AddToRoleAsync(user, newRole);
                if (!added.Succeeded)
                {
                    return (false, added.Errors.Select(e => e.Description));
                }
            }

            // Remove other roles
            var toRemove = currentRoles.Where(r => !string.Equals(r, newRole, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (toRemove.Length > 0)
            {
                var removed = await userManager.RemoveFromRolesAsync(user, toRemove);
                if (!removed.Succeeded)
                {
                    return (false, removed.Errors.Select(e => e.Description));
                }
            }
        }

        return (true, null);
    }

    public async Task<(bool ok, IEnumerable<string>? errors)> DeleteAsync(int id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return (false, new[] { $"User {id} not found" });
        }

        var result = await userManager.DeleteAsync(user);
        return (result.Succeeded, result.Succeeded ? null : result.Errors.Select(e => e.Description));
    }

    private static UserReadDto ToDto(User user, IEnumerable<string> roles)
        => new(user.Id, user.Login, user.FullName, user.Email, user.PhoneNumber, roles.ToArray());
}
