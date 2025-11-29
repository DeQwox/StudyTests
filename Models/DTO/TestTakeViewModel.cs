using System.ComponentModel.DataAnnotations;

namespace StudyTests.Models.DTO;

public class TestTakeViewModel
{
    [Required]
    public int TestId { get; set; }

    // Step state
    public int Index { get; set; } // current question index (0-based)
    public int Total { get; set; }

    // For initial start
    [Display(Name = "Пароль тесту")]
    public string? Password { get; set; }

    // For question rendering
    public string TestName { get; set; } = string.Empty;
    public string TestDescription { get; set; } = string.Empty;

    public string QuestionText { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();

    // Selected answer for current index
    public int? Selected { get; set; }

    // Carry state between posts as JSON of int[] (selected answers per index; -1 for unanswered)
    public string AnswersJson { get; set; } = string.Empty;

    // For final result
    public double? Score { get; set; }
    public double? MaxScore { get; set; }
}
