using System.ComponentModel.DataAnnotations;

namespace StudyTests.Models.DTO.Authorization
{
    public class RegisterApiDto
    {
        [Required]
        public string Login { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? FullName { get; set; }
        [Required]
        public string Role { get; set; } = "Student";
    }
}
