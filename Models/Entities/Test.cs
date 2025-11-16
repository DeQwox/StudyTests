using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyTests.Models.Entities;

public class Test : Entity
{
    [Required]
    public int TeacherID { get; set; }
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ValidUntil { get; set; }
    
    [MaxLength(200)]
    public string? Password { get; set; }

    [ForeignKey(nameof(TeacherID))]
    public User Teacher { get; set; } = null!;

    public ICollection<Question> Questions { get; set; }  = [];
}