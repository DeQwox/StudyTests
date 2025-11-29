using StudyTests.Models.DTO.Api;
using StudyTests.Models.Entities;

namespace StudyTests.Services.Api;

public interface IQuestionsService
{
    Task<IEnumerable<Question>> GetAllAsync(int? testId);
    Task<Question?> GetByIdAsync(int id);
    Task<Question?> CreateAsync(QuestionDto dto);
    Task<bool> UpdateAsync(int id, QuestionDto dto);
    Task<bool> DeleteAsync(int id);
}
