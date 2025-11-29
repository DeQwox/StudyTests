using StudyTests.Models.DTO.Api;

namespace StudyTests.Services.Api;

public interface IUsersService
{
    Task<IEnumerable<UserReadDto>> GetAllAsync(string? role = null);
    Task<UserReadDto?> GetByIdAsync(int id);
    Task<(bool ok, UserReadDto? result, IEnumerable<string>? errors)> CreateAsync(CreateUserDto dto);
    Task<(bool ok, IEnumerable<string>? errors)> UpdateAsync(int id, UpdateUserDto dto);
    Task<(bool ok, IEnumerable<string>? errors)> DeleteAsync(int id);
}
