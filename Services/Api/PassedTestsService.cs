using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using StudyTests.Data;
using StudyTests.Models.DTO.Api;
using StudyTests.Models.Entities;
using System.Threading;

namespace StudyTests.Services.Api;

public class PassedTestsService(ApplicationDbContext db, ILogger<PassedTestsService> logger, IServiceScopeFactory scopeFactory, DbContextOptions<ApplicationDbContext> dbOptions) : IPassedTestsService
{
    private readonly ApplicationDbContext _db = db;
    private readonly ILogger<PassedTestsService> _logger = logger;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly DbContextOptions<ApplicationDbContext> _dbOptions = dbOptions;
    
    // Global semaphore to serialize writes to the SQLite database
    private static readonly SemaphoreSlim _writeSemaphore = new(1, 1);

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
        _logger.LogInformation("Attempting to create PassedTest. StudentId: {StudentId}, TestId: {TestId}", dto.StudentId, dto.TestId);

        // Acquire semaphore to ensure only one operation happens at a time globally
        await _writeSemaphore.WaitAsync();
        try
        {
            const int MaxRetries = 5;
            int delayMs = 200;

            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    // Create a transient context for the ENTIRE operation
                    using var context = new ApplicationDbContext(_dbOptions);
                    
                    // Explicitly open connection to set timeout
                    await context.Database.OpenConnectionAsync();
                    
                    // Set a generous timeout
                    using (var cmd = context.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "PRAGMA busy_timeout = 5000;";
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // 1. Validate Student
                    if (dto.StudentId.HasValue)
                    {
                        var userExists = await context.Users.AsNoTracking().AnyAsync(u => u.Id == dto.StudentId.Value);
                        if (!userExists)
                        {
                             _logger.LogWarning("User with ID {StudentId} not found in Users table. Checking Students table...", dto.StudentId);
                             if (!await context.Students.AsNoTracking().AnyAsync(s => s.Id == dto.StudentId.Value))
                             {
                                 _logger.LogWarning("Student with ID {StudentId} not found.", dto.StudentId);
                                 return null;
                             }
                        }
                    }
                    
                    // 2. Validate Test
                    var testExists = await context.Tests.AsNoTracking().AnyAsync(t => t.Id == dto.TestId);
                    if (!testExists) 
                    {
                        _logger.LogWarning("Test with ID {TestId} not found.", dto.TestId);
                        return null;
                    }

                    // 3. Calculate score (Read questions)
                    var questions = await context.Questions
                        .AsNoTracking()
                        .Where(q => q.TestId == dto.TestId)
                        .OrderBy(q => q.Id)
                        .ToListAsync();

                    double calculatedScore = 0;
                    int count = Math.Min(questions.Count, dto.Answers.Count);
                    for (int i = 0; i < count; i++)
                    {
                        var question = questions[i];
                        var clientAnswer = dto.Answers[i];

                        if (question.Answers != null && 
                            question.CorrectAnswerIndex >= 0 && 
                            question.CorrectAnswerIndex < question.Answers.Count)
                        {
                            var correctAnswerText = question.Answers[question.CorrectAnswerIndex];
                            if (string.Equals(clientAnswer?.Trim(), correctAnswerText?.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                calculatedScore += question.Score;
                            }
                        }
                    }

                    _logger.LogInformation("Calculated score for Test {TestId}: {Score}. Writing to database...", dto.TestId, calculatedScore);

                    // 4. Write
                    var p = new Models.Entities.PassedTest
                    {
                        StudentId = dto.StudentId,
                        TestId = dto.TestId,
                        Answers = dto.Answers,
                        Score = calculatedScore,
                        PassedAt = dto.PassedAt ?? DateTime.UtcNow
                    };
                    
                    context.PassedTests.Add(p);
                    await context.SaveChangesAsync();
                    
                    _logger.LogInformation("PassedTest created successfully. ID: {Id}", p.Id);
                    return p;
                }
                catch (Exception ex) when (IsSqliteLocked(ex))
                {
                    _logger.LogWarning(ex, "Database locked during attempt {Attempt}/{MaxRetries}. Retrying in {Delay}ms...", attempt, MaxRetries, delayMs);
                    
                    // Force clear pools to drop any stuck connections
                    SqliteConnection.ClearAllPools();
                    
                    if (attempt == MaxRetries) throw;
                    
                    await Task.Delay(delayMs);
                    delayMs *= 2; // Exponential backoff
                }
            }
        }
        finally
        {
            _writeSemaphore.Release();
            _logger.LogInformation("Write semaphore released.");
        }
        return null;
    }

    private static bool IsSqliteLocked(Exception ex)
    {
        if (ex is SqliteException sqlEx && (sqlEx.SqliteErrorCode == 5 || sqlEx.SqliteErrorCode == 6)) return true;
        if (ex is DbUpdateException dbEx && dbEx.InnerException is SqliteException innerSqlEx && (innerSqlEx.SqliteErrorCode == 5 || innerSqlEx.SqliteErrorCode == 6)) return true;
        return false;
    }

    public async Task<bool> UpdateAsync(int id, PassedTestDto dto)
    {
        var p = await _db.PassedTests.FindAsync(id);
        if (p == null) return false;
        p.StudentId = dto.StudentId; p.TestId = dto.TestId; p.Answers = dto.Answers; p.Score = dto.Score; p.PassedAt = dto.PassedAt ?? p.PassedAt;
        await SaveChangesWithRetryAsync(_db);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var p = await _db.PassedTests.FindAsync(id);
        if (p == null) return false;
        _db.PassedTests.Remove(p);
        await SaveChangesWithRetryAsync(_db);
        return true;
    }

    private async Task SaveChangesWithRetryAsync(ApplicationDbContext dbContext, int maxAttempts = 5, int initialDelayMs = 200)
    {
        var attempt = 0;
        var delay = initialDelayMs;
        while (true)
        {
            attempt++;
            try
            {
                await dbContext.SaveChangesAsync();
                return;
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqliteException sqliteEx && 
                (sqliteEx.SqliteErrorCode == 5 || sqliteEx.SqliteErrorCode == 6 || sqliteEx.SqliteErrorCode == 261 || sqliteEx.SqliteErrorCode == 208))
            {
                _logger?.LogWarning(ex, "SQLite busy/locked on SaveChanges (attempt {Attempt}/{MaxAttempts}). Retrying after {Delay}ms.", attempt, maxAttempts, delay);
                SqliteConnection.ClearAllPools(); // Try to clear pools to release locks
                if (attempt >= maxAttempts) throw;
                await Task.Delay(delay);
                delay *= 2;
                continue;
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 5 || ex.SqliteErrorCode == 6 || ex.SqliteErrorCode == 261 || ex.SqliteErrorCode == 208)
            {
                _logger?.LogWarning(ex, "SQLite busy/locked on SaveChanges (attempt {Attempt}/{MaxAttempts}). Retrying after {Delay}ms.", attempt, maxAttempts, delay);
                SqliteConnection.ClearAllPools(); // Try to clear pools to release locks
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
