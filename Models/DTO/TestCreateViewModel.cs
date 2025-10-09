using System.ComponentModel.DataAnnotations;
using StudyTests.Models.Entities;

namespace Models.DTO
{
    public class TestCreateViewModel
    {
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public DateTime? ValidUntil { get; set; }

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public int TeacherId { get; set; }

        // Simple representation for questions in the form (JSON string or use JS to post)
        public List<Question> Questions { get; set; } = new List<Question>();
        // JSON string posted from the form with questions
        public string? QuestionsJson { get; set; }
    }
}
