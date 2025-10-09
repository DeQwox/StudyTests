using StudyTests.Models.Entities;
using Models.DTO;

namespace Repositories;

public interface ITestingRepository
{
    public Task<IEnumerable<Test>> GetAllTestsAsync();
    public Task<Test?> GetTestByIdAsync(int id);
    public Task<Student?> GetStudentByIdAsync(int id);
    public Task<Teacher?> GetTeacherByIdAsync(int id);
    public Test? GetTestById(int id);
    public IEnumerable<Question> GetTestQuestionList(int id);
        // Added for creating tests and listing teachers
    public Task AddTestAsync(TestCreateViewModel model);
    public Task<IEnumerable<Teacher>> GetAllTeachersAsync();
}

