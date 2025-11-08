using StudyTests.Models.DTO.Api;
using StudyTests.Models.Entities;

namespace StudyTests.Services.Api;

public interface IPassedTestsService
{
    Task<IEnumerable<PassedTest>> GetAllAsync(int? studentId, int? testId);
    Task<PassedTest?> GetByIdAsync(int id);
    Task<PassedTest?> CreateAsync(PassedTestDto dto);
    Task<bool> UpdateAsync(int id, PassedTestDto dto);
    Task<bool> DeleteAsync(int id);
}
