<<<<<<< HEAD
using Data;
using Microsoft.EntityFrameworkCore;
using StudyTests.Models.Entities;
=======
using StudyTests.Models.Entities;
using StudyTests.Models.DTO.Tests;
>>>>>>> origin/lab6Artem

namespace Repositories;

public interface ITestingRepository
{
    public Task<IEnumerable<Test>> GetAllTestsAsync();
    public Task<Test?> GetTestByIdAsync(int id);
<<<<<<< HEAD
    public Task<Student?> GetStudentByIdAsync(int id);
    public Task<Teacher?> GetTeacherByIdAsync(int id);
    public Test? GetTestById(int id);
    public IEnumerable<Question> GetTestQuestionList(int id);
=======
    public Task<IEnumerable<PassedTest>> GePassedTestsAsync();
    public Task<PassedTest?> GePassedTestByIdAsync(int id);
    public Task<IEnumerable<PassedTest>> GePassedTestsByTestAsync(int testId);
    public Task<IEnumerable<PassedTest>> GePassedTestsByStudentAsync(int studentId);
    public Task<PassedTest?> GetPassedTestByStudentAndTestAsync(int studentId, int testId);
    public Task<User?> GetStudentByIdAsync(int id);
    public Task<User?> GetTeacherByIdAsync(int id);
    public Test? GetTestById(int id);
    public IEnumerable<Question> GetTestQuestionList(int id);
    public Task AddTestAsync(TestCreateViewModel model);
    public Task AddPassedTestAsync(PassedTest passedTest);
    public Task<IEnumerable<Teacher>> GetAllTeachersAsync();

    public Task<bool> RemoveTestAsync(int id);
>>>>>>> origin/lab6Artem
}

