using Microsoft.EntityFrameworkCore;
using StudyTests.Data;
using StudyTests.Models.DTO.Api;
using StudyTests.Models.Entities;

namespace StudyTests.Services.Api;

public class PassedTestsService(ApplicationDbContext db) : IPassedTestsService
{
    private readonly ApplicationDbContext _db = db;

    public async Task<IEnumerable<PassedTest>> GetAllAsync(int? studentId, int? testId)
    {
        var query = _db.PassedTests.AsQueryable();
        if (studentId.HasValue) query = query.Where(p => p.StudentId == studentId.Value);
        if (testId.HasValue) query = query.Where(p => p.TestId == testId.Value);
        return await query.ToListAsync();
    }

    public async Task<PassedTest?> GetByIdAsync(int id)
    {
        return await _db.PassedTests.FindAsync(id);
    }

    public async Task<PassedTest?> CreateAsync(PassedTestDto dto)
    {
        var studentExists = await _db.Students.AnyAsync(s => s.Id == dto.StudentId);
        var testExists = await _db.Tests.AnyAsync(t => t.Id == dto.TestId);
        if (!studentExists || !testExists) return null;
        var p = new Models.Entities.PassedTest { StudentId = dto.StudentId, TestId = dto.TestId, Answers = dto.Answers, Score = dto.Score, PassedAt = dto.PassedAt ?? DateTime.UtcNow };
        _db.PassedTests.Add(p);
        await _db.SaveChangesAsync();
        return p;
    }

    public async Task<bool> UpdateAsync(int id, PassedTestDto dto)
    {
        var p = await _db.PassedTests.FindAsync(id);
        if (p == null) return false;
        p.StudentId = dto.StudentId; p.TestId = dto.TestId; p.Answers = dto.Answers; p.Score = dto.Score; p.PassedAt = dto.PassedAt ?? p.PassedAt;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var p = await _db.PassedTests.FindAsync(id);
        if (p == null) return false;
        _db.PassedTests.Remove(p);
        await _db.SaveChangesAsync();
        return true;
    }
}
