namespace StudyTests.Models.DTO.Api;

public record UserReadDto(int Id, string Login, string FullName, string? Email, string? PhoneNumber, string[] Roles);
public record CreateUserDto(string Login, string FullName, string Email, string PhoneNumber, string Password, string Role);
public record UpdateUserDto(string Login, string FullName, string Email, string PhoneNumber, string? Role);
