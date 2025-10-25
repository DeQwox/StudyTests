using System.ComponentModel.DataAnnotations;

namespace StudyTests.Models.DTO.Authorization;

public class RegisterViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Введіть ім'я користувача")]
    [StringLength(50, ErrorMessage = "Максимум 50 символів")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть прізвище ім'я")]
    [StringLength(500, ErrorMessage = "Максимум 500 символів")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть email")]
    [EmailAddress(ErrorMessage = "Невірний формат email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть номер телефону")]
    [RegularExpression(@"^(?:\+380\s?\d{9}|\d{10})$", ErrorMessage = "Формат телефону: +380XXXXXXXXX або +380 XXXXXXXXX або XXXXXXXXXX")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть пароль")]
    // [StringLength(16, MinimumLength = 8, ErrorMessage = "Пароль має бути від 8 до 16 символів")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).{8,16}$",
    ErrorMessage = "Пароль має містити мінімум 8 символів, максимум 16, серед яких принаймні 1 велика літера, 1 цифра та 1 спеціальний символ")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Підтвердіть пароль")]
    [Compare("Password", ErrorMessage = "Паролі не співпадають")]
    public string ConfirmPassword { get; set; } = string.Empty;
    
    public string Role { get; set; } = "Student";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();
        var email = (Email ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(email))
            return results;

        var atIndex = email.IndexOf('@');
        if (atIndex <= 0 || atIndex == email.Length - 1)
        {
            results.Add(new ValidationResult("Невірний формат email", [nameof(Email)]));
            return results;
        }

        var domain = email[(atIndex + 1)..].ToLowerInvariant();
        if (domain != "gmail.com")
        {
            results.Add(new ValidationResult("Дозволено лише адреси @gmail.com", [nameof(Email)]));
        }

        return results;
    }
}
