using Microsoft.EntityFrameworkCore;
using StudyTests.Data;
using StudyTests.Models.DTO.Api;
using StudyTests.Models.Entities;

namespace StudyTests.Services.Api;

public class QuestionsService(ApplicationDbContext db) : IQuestionsService
{
    private readonly ApplicationDbContext _db = db;

    public async Task<IEnumerable<Question>> GetAllAsync(int? testId)
    {
        var query = _db.Questions.AsQueryable();
        if (testId.HasValue) query = query.Where(q => q.TestId == testId.Value);
        return await query.ToListAsync();
    }

    public async Task<Question?> GetByIdAsync(int id)
    {
        return await _db.Questions.FindAsync(id);
    }

    public async Task<Question?> CreateAsync(QuestionDto dto)
    {
        var q = new Models.Entities.Question
        {
            TestId = dto.TestId,
            Description = dto.Description,
            Answers = dto.Answers,
            CorrectAnswerIndex = dto.CorrectAnswerIndex,
            Score = dto.Score
        };
        _db.Questions.Add(q);
        await _db.SaveChangesAsync();
        return q;
    }

    public async Task<bool> UpdateAsync(int id, QuestionDto dto)
    {
        var q = await _db.Questions.FindAsync(id);
        if (q == null) return false;
        q.TestId = dto.TestId; q.Description = dto.Description; q.Answers = dto.Answers; q.CorrectAnswerIndex = dto.CorrectAnswerIndex; q.Score = dto.Score;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var q = await _db.Questions.FindAsync(id);
        if (q == null) return false;
        _db.Questions.Remove(q);
        await _db.SaveChangesAsync();
        return true;
    }
}
