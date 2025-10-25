using StudyTests.Models.Entities;
using Models.DTO.Authorization;

namespace StudyTests.Repositories;

public interface IAccountRepository
{
    Task AddStudentAsync(Student student);
    Task AddUserAsync(RegisterViewModel model);
    Task<User?> LoginUserAsync(LoginViewModel loginViewModel);
}

