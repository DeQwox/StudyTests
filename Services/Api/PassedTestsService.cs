using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using StudyTests.Data;
using StudyTests.Models.DTO.Api;
using StudyTests.Models.Entities;

namespace StudyTests.Services.Api;

public class PassedTestsService(ApplicationDbContext db, ILogger<PassedTestsService> logger) : IPassedTestsService
{
    private readonly ApplicationDbContext _db = db;
    private readonly ILogger<PassedTestsService> _logger = logger;

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
        // Validate student and test existence. Use FindAsync on Users and Tests to accept Identity users
        // (without relying on the Student discriminator). This reduces extra EXISTS queries.
        var user = await _db.Users.FindAsync(dto.StudentId);
        var test = await _db.Tests.FindAsync(dto.TestId);
        if (user == null && !await _db.Students.AnyAsync(s => s.Id == dto.StudentId)) return null;
        if (test == null) return null;

        var p = new Models.Entities.PassedTest
        {
            StudentId = dto.StudentId,
            TestId = dto.TestId,
            Answers = dto.Answers,
            Score = dto.Score,
            PassedAt = dto.PassedAt ?? DateTime.UtcNow
        };
        _db.PassedTests.Add(p);
        // Save with retry to handle transient SQLite 'database is locked' errors in dev scenarios
        await SaveChangesWithRetryAsync();
        return p;
    }

    public async Task<bool> UpdateAsync(int id, PassedTestDto dto)
    {
        var p = await _db.PassedTests.FindAsync(id);
        if (p == null) return false;
        p.StudentId = dto.StudentId; p.TestId = dto.TestId; p.Answers = dto.Answers; p.Score = dto.Score; p.PassedAt = dto.PassedAt ?? p.PassedAt;
        await SaveChangesWithRetryAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var p = await _db.PassedTests.FindAsync(id);
        if (p == null) return false;
        _db.PassedTests.Remove(p);
        await SaveChangesWithRetryAsync();
        return true;
    }

    private async Task SaveChangesWithRetryAsync(int maxAttempts = 5, int initialDelayMs = 200)
    {
        var attempt = 0;
        var delay = initialDelayMs;
        while (true)
        {
            attempt++;
            try
            {
                await _db.SaveChangesAsync();
                return;
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 5 /* SQLITE_BUSY */ || ex.SqliteErrorCode == 261 /* SQLITE_BUSY_RECOVERY or similar */ || ex.SqliteErrorCode == 208 /* locked */)
            {
                _logger?.LogWarning(ex, "SQLite busy/locked on SaveChanges (attempt {Attempt}/{MaxAttempts}). Retrying after {Delay}ms.", attempt, maxAttempts, delay);
                if (attempt >= maxAttempts) throw;
                await Task.Delay(delay);
                delay *= 2;
                continue;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error saving changes to the database.");
                throw;
            }
        }
    }
}
