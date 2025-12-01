using System;
using System.Collections.Generic;

namespace StudyTests.Models.DTO.Api;

public record QuestionCreateDto(string Description, List<string> Answers, int CorrectAnswerIndex, double Score);

public record TestWithQuestionsCreateDto(
    string Name,
    string? Description,
    string? Password,
    DateTime? ValidUntil,
    List<QuestionCreateDto> Questions
);