using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace StudyTests.Models.Entities;

[Index(nameof(Email), IsUnique = true)]
[Index(nameof(PhoneNumber), IsUnique = true)]
public class User : IdentityUser<int>
{
    [Required, MaxLength(50)]
    public string Login { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(255)]
    public override string? Email { get; set; } = string.Empty;

    [Required, MaxLength(20), Phone]
    public override string? PhoneNumber { get; set; } = string.Empty;

    [Required, MaxLength(255), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
