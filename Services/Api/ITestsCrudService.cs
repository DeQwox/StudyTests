using StudyTests.Models.DTO.Api;

namespace StudyTests.Services.Api;

public interface ITestsCrudService
{
    Task<IEnumerable<TestReadDto>> GetAllAsync();
    Task<TestReadDto?> GetByIdAsync(int id);
    Task<TestReadDto?> CreateAsync(TestDto dto);
    Task<bool> UpdateAsync(int id, TestDto dto);
    Task<bool> DeleteAsync(int id);
}
