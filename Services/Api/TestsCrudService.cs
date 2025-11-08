using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using StudyTests.Data;
using StudyTests.Models.DTO.Api;
using StudyTests.Models.Entities;

namespace StudyTests.Services.Api;

public class TestsCrudService(ApplicationDbContext db, UserManager<User> userManager) : ITestsCrudService
{
    private readonly ApplicationDbContext _db = db;
    private readonly UserManager<User> _userManager = userManager;

    public async Task<IEnumerable<TestReadDto>> GetAllAsync()
    {
        return await _db.Tests
            .Select(t => new TestReadDto(t.Id, t.TeacherID, t.Name, t.Description, t.CreatedAt, t.ValidUntil))
            .ToListAsync();
    }

    public async Task<TestReadDto?> GetByIdAsync(int id)
    {
        var t = await _db.Tests.FindAsync(id);
        return t == null ? null : new TestReadDto(t.Id, t.TeacherID, t.Name, t.Description, t.CreatedAt, t.ValidUntil);
    }

    public async Task<TestReadDto?> CreateAsync(TestDto dto)
    {
        // Validate teacher exists and is in Teacher role
        var teacher = await _userManager.FindByIdAsync(dto.TeacherID.ToString());
        if (teacher == null) return null;
        if (!await _userManager.IsInRoleAsync(teacher, "Teacher")) return null;

        var t = new Test
        {
            TeacherID = dto.TeacherID,
            Name = dto.Name,
            Description = dto.Description,
            Password = dto.Password,
            CreatedAt = DateTime.UtcNow,
            ValidUntil = dto.ValidUntil
        };
        _db.Tests.Add(t);
        await _db.SaveChangesAsync();
        return new TestReadDto(t.Id, t.TeacherID, t.Name, t.Description, t.CreatedAt, t.ValidUntil);
    }

    public async Task<bool> UpdateAsync(int id, TestDto dto)
    {
        var t = await _db.Tests.FindAsync(id);
        if (t == null) return false;
        t.TeacherID = dto.TeacherID;
        t.Name = dto.Name;
        t.Description = dto.Description;
        t.Password = dto.Password;
        t.ValidUntil = dto.ValidUntil;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var t = await _db.Tests.FindAsync(id);
        if (t == null) return false;
        _db.Tests.Remove(t);
        await _db.SaveChangesAsync();
        return true;
    }
}
