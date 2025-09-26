using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace StudyTests.Models.Entities;

[Index(nameof(Login), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
public class User: Entity
{
    [Required]
    [MaxLength(200)]
    public string Login { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

}