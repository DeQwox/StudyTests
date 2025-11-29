using System.ComponentModel.DataAnnotations;

namespace StudyTests.Models.DTO.Authorization;

public class LoginViewModel
{
    [Required(ErrorMessage = "Поле логін обов'язкове")]
    public string? Login { get; set; }

    [Required(ErrorMessage = "Поле пароль обов'язкове"), DataType(DataType.Password)]
    public string? Password { get; set; }

    public string? Role { get; set; }
}