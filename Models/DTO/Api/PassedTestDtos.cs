namespace StudyTests.Models.DTO.Api;

public record PassedTestDto(int StudentId, int TestId, List<string> Answers, double Score, DateTime? PassedAt);
