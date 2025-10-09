using System.ComponentModel.DataAnnotations;

namespace Models.DTO.Authorization;

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Поле ім'я користувача обов'язкове")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Поле пароль обов'язкове")]
        public string Password { get; set; }

        public string Role { get; set; }
    }
