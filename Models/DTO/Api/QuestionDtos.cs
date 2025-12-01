namespace StudyTests.Models.DTO.Api;

public record QuestionDto(int TestId, string Description, List<string> Answers, int CorrectAnswerIndex, double Score);
public record QuestionReadDto(int Id, int TestId, string Description, List<string> Answers, int CorrectAnswerIndex, double Score);
public record QuestionStudentDto(int Id, int TestId, string Description, List<string> Answers, double Score);

