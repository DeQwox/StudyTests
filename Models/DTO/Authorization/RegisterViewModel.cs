using System.ComponentModel.DataAnnotations;

namespace Models.DTO.Authorization
{
    public class RegisterViewModel : System.ComponentModel.DataAnnotations.IValidatableObject
    {
        [Required(ErrorMessage = "Введіть ім'я користувача")]
        [StringLength(50, ErrorMessage = "Максимум 50 символів")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введіть ім'я")]
        [StringLength(500, ErrorMessage = "Максимум 500 символів")]
    public string FirstName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Введіть прізвище")]
        [StringLength(500, ErrorMessage = "Максимум 500 символів")]
    public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введіть email")]
        [EmailAddress(ErrorMessage = "Невірний формат email")]
    public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введіть номер телефону")]
    // Accepts: +380XXXXXXXXX  or  "+380 XXXXXXXXX" (space after country code)  or  XXXXXXXXXX (10 digits)
    [RegularExpression(@"^(?:\+380\s?\d{9}|\d{10})$", ErrorMessage = "Формат телефону: +380XXXXXXXXX або +380 XXXXXXXXX або XXXXXXXXXX")]
    public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введіть пароль")]
        [StringLength(16, MinimumLength = 8, ErrorMessage = "Пароль має бути від 8 до 16 символів")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).+$", ErrorMessage = "Пароль має містити 1 велику літеру, 1 цифру та 1 спеціальний символ")]
    public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Підтвердіть пароль")]
        [Compare("Password", ErrorMessage = "Паролі не співпадають")]
        public string ConfirmPassword { get; set; } = string.Empty;
        
        public string Role { get; set; } = "Student";

        public IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
        {
            var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            var email = (Email ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(email))
                return results; // Required attribute handles empties

            var atIndex = email.IndexOf('@');
            if (atIndex <= 0 || atIndex == email.Length - 1)
            {
                results.Add(new System.ComponentModel.DataAnnotations.ValidationResult("Невірний формат email", new[] { nameof(Email) }));
                return results;
            }

            var domain = email[(atIndex + 1)..].ToLowerInvariant();
            if (domain != "gmail.com")
            {
                results.Add(new System.ComponentModel.DataAnnotations.ValidationResult("Дозволено лише адреси @gmail.com", new[] { nameof(Email) }));
            }

            return results;
        }
    }
}
