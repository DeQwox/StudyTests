using StudyTests.Models.Entities;
using StudyTests.Models.DTO.Authorization;
using Microsoft.AspNetCore.Identity;

namespace StudyTests.Repositories;

public interface IAccountRepository
{
    Task AddStudentAsync(Student student);
    Task AddUserAsync(RegisterViewModel model);
    Task<User?> LoginUserAsync(LoginViewModel loginViewModel);

    Task<User?> FindUserByEmailAsync(string email);
    Task LinkExternalLoginAsync(User localUser, ExternalLoginInfo info);
}
