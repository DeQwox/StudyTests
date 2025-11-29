namespace StudyTests.Models.DTO.Authorization;

public class IdentityServerLoginViewModel
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string ReturnUrl { get; set; } = "";
}
