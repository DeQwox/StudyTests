namespace StudyTests.Models.DTO.Api;

public record TestDto(int TeacherID, string Name, string Description, string Password, DateTime? ValidUntil);
public record TestReadDto(int Id, int TeacherID, string Name, string Description, DateTime CreatedAt, DateTime? ValidUntil);
