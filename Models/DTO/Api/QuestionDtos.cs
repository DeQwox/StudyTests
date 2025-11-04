namespace StudyTests.Models.DTO.Api;

public record QuestionDto(int TestId, string Description, List<string> Answers, int CorrectAnswerIndex, double Score);
