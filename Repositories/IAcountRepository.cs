using StudyTests.Models.Entities;
using Models.DTO.Authorization;

namespace Repositories;

public interface IAcountRepository
{
    Task AddStudentAsync(Student student);
    Task AddUserAsync(RegisterViewModel model);
    Task<User?> LoginUserAsync(LoginViewModel loginViewModel);
}

