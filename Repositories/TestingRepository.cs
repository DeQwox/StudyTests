using StudyTests.Data;
using Microsoft.EntityFrameworkCore;
using StudyTests.Models.DTO.Tests;
using StudyTests.Models.Entities;


namespace Repositories;

public class TestingRepository(ApplicationDbContext context) : ITestingRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<Test>> GetAllTestsAsync()
    {
        return await _context.Tests
            .Include(t => t.Questions)
            .Include(t => t.Teacher)
            .ToListAsync();
    }

    public async Task<Test?> GetTestByIdAsync(int id)
    {
        return await _context.Tests
            .Include(t => t.Teacher)
            .Include(t => t.Questions)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<PassedTest>> GePassedTestsAsync()
    {
        return await _context.PassedTests
            .Include(t => t.Test)
            .ToListAsync();
    }

    public async Task<PassedTest?> GePassedTestByIdAsync(int id)
    {
        return await _context.PassedTests
            .Include(t => t.Test)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<PassedTest>> GePassedTestsByTestAsync(int testId)
    {
        return await _context.PassedTests
            .Include(t => t.Test)
            .Where(p => p.TestId == testId)
            .ToListAsync();
    }

    public async Task<IEnumerable<PassedTest>> GePassedTestsByStudentAsync(int studentId)
    {
        return await _context.PassedTests
            .Include(t => t.Test)
            .Where(p => p.StudentId == studentId)
            .ToListAsync();
    }


    public async Task<PassedTest?> GetPassedTestByStudentAndTestAsync(int studentId, int testId)
    {
        return await _context.PassedTests
            .Include(t => t.Test)
            .FirstOrDefaultAsync(i => i.StudentId == studentId && i.TestId == testId);
    }


    public async Task<User?> GetStudentByIdAsync(int id)
    {
        return await _context.Users.FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<User?> GetTeacherByIdAsync(int id)
    {
        return await _context.Users.FirstOrDefaultAsync(i => i.Id == id);
    }

    public Test? GetTestById(int id)
    {
        return _context.Tests
            .Include(t => t.Questions)
            .Include(t => t.Teacher)
            .FirstOrDefault(i => i.Id == id);
    }

    public IEnumerable<Question> GetTestQuestionList(int id)
    {
        return _context.Questions.Include(i => i.Test).Where(i => i.TestId == id);
    }

    public async Task AddTestAsync(TestCreateViewModel model)
    {
        var test = new Test
        {
            Name = model.Name,
            Description = model.Description,
            Password = model.Password,
            ValidUntil = model.ValidUntil,
            TeacherID = model.TeacherId
        };

        await _context.Tests.AddAsync(test);
        await _context.SaveChangesAsync();

        if (model.Questions != null && model.Questions.Count != 0)
        {
            var questions = model.Questions.Select(q => new Question
            {
                TestId = test.Id,
                Description = q.Description,
                Answers = q.Answers,
                CorrectAnswerIndex = q.CorrectAnswerIndex,
                Score = q.Score
            }).ToList();

            await _context.Questions.AddRangeAsync(questions);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddPassedTestAsync(PassedTest passedTest)
    {
        _context.PassedTests.Add(passedTest);
        await _context.SaveChangesAsync();
    }


    public async Task<IEnumerable<Teacher>> GetAllTeachersAsync()
    {
        return await _context.Teachers.ToListAsync();
    }


    public async Task<bool> RemoveTestAsync(int id)
    {
        var test = await GetTestByIdAsync(id);

        if (test is null) return false;

        var passedTests = await GePassedTestsByTestAsync(id);

        _context.PassedTests.RemoveRange(passedTests);
        _context.Tests.Remove(test);

        await _context.SaveChangesAsync();

        return true;
    }

}

