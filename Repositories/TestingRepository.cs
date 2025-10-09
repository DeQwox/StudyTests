using Data;
using Microsoft.EntityFrameworkCore;
using Models.DTO;
using StudyTests.Models.Entities;

namespace Repositories;

public class TestingRepository(ApplicationDbContext context) : ITestingRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<Test>> GetAllTestsAsync()
    {
        return await _context.Tests.ToListAsync();
    }

    public async Task<Test?> GetTestByIdAsync(int id)
    {
        return await _context.Tests.FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Student?> GetStudentByIdAsync(int id)
    {
        return await _context.Students.FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Teacher?> GetTeacherByIdAsync(int id)
    {
        return await _context.Teachers.FirstOrDefaultAsync(i => i.Id == id);
    }

    public Test? GetTestById(int id)
    {
        return _context.Tests.FirstOrDefault(i => i.Id == id);
    }

    public IEnumerable<Question> GetTestQuestionList(int id)
    {
        return _context.Questions.Where(i => i.TestId == id);
    }

    public async Task AddTestAsync(TestCreateViewModel model)
    {
        var test = new Test
        {
            Name = model.Name,
            Description = model.Description,
            Password = model.Password,
            ValidUntil = model.ValidUntil,
            TeacherID = model.TeacherId,
            Questions = model.Questions.Select(q => new Question
            {
                Description = q.Description,
                Answers = q.Answers,
                CorrectAnswerIndex = q.CorrectAnswerIndex,
                Score = q.Score
            }).ToList()
        };

        await _context.Tests.AddAsync(test);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Teacher>> GetAllTeachersAsync()
    {
        return await _context.Teachers.ToListAsync();
    }

    // public async Task AddAsync(T entity)
    // {
    //     await _dbSet.AddAsync(entity);
    //     await _context.SaveChangesAsync();
    // }

    // public async Task UpdateAsync(T entity)
    // {
    //     _dbSet.Update(entity);
    //     await _context.SaveChangesAsync();
    // }

    // public async Task DeleteAsync(int id)
    // {
    //     var entity = await GetByIdAsync(id);
    //     if (entity != null)
    //     {
    //         _dbSet.Remove(entity);
    //         await _context.SaveChangesAsync();
    //     }
    // }
}

