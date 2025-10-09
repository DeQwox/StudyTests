using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace StudyTests.Models.Entities;

[Index(nameof(Login), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
[Index(nameof(PhoneNumber), IsUnique = true)]
public class User : Entity
{
    [Required]
    [MaxLength(200)]
    public string Login { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = null!;
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = null!;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    [Phone]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;
}