using StudyTests.Models.Entities;

namespace StudyTests.Models.DTO.Api1
{
    // For the list of tests
    public class TestSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TeacherName { get; set; }
        public string TeacherId { get; set; }
        public double MaxScore { get; set; }
        public PassedInfo? Passed { get; set; } // Null if not passed
    }

    public class PassedInfo 
    {
        public double Score { get; set; }
        public DateTime PassedAt { get; set; }
    }

    // For detailed view
    public class TestDetailDto : TestSummaryDto
    {
        public int QuestionCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ValidUntil { get; set; }
        public bool IsOwner { get; set; } // To show delete button
    }

    // For creating a test
    public class CreateTestDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string? Password { get; set; }
        public DateTime? ValidUntil { get; set; }
        public List<QuestionDto> Questions { get; set; }
    }

    public class QuestionDto
    {
        public string Description { get; set; }
        public double Score { get; set; }
        public List<string> Answers { get; set; }
        public int CorrectAnswerIndex { get; set; }
    }
}