using StudyTests.Data;
using StudyTests.Models.DTO.Authorization;
using StudyTests.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace StudyTests.Repositories;

public class AccountRepository(ApplicationDbContext context, UserManager<User> userManager) : IAccountRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly UserManager<User> _userManager = userManager;

    public async Task AddStudentAsync(Student student)
    {
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();
    }

    public async Task AddUserAsync(RegisterViewModel model)
    {
        if (model.Role == "Student")
        {
            var student = new Student
            {
                Login = model.Login,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Password = model.Password
            };
            await _context.Students.AddAsync(student);
        }
        else if (model.Role == "Teacher")
        {
            var teacher = new Teacher
            {
                Login = model.Login,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Password = model.Password
            };
            await _context.Teachers.AddAsync(teacher);
        }
        await _context.SaveChangesAsync();
    }
    public async Task<User?> LoginUserAsync(LoginViewModel loginViewModel)
    {
        if (loginViewModel.Role == "Student")
        {
            return await _context.Students
                .FirstOrDefaultAsync(u => u.Login == loginViewModel.Login && u.Password == loginViewModel.Password);
        }
        else if (loginViewModel.Role == "Teacher")
        {
            return await _context.Teachers
                .FirstOrDefaultAsync(u => u.Login == loginViewModel.Login && u.Password == loginViewModel.Password);
        }
        return null;
    }

    public async Task<User?> FindUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task LinkExternalLoginAsync(User localUser, ExternalLoginInfo info)
    {
        var login = new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName);
        await _userManager.AddLoginAsync(localUser, login);
    }
}