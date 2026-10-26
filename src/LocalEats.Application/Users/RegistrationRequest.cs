using System.ComponentModel.DataAnnotations;

namespace LocalEats.Application.Users;

public sealed class RegistrationRequest
{
    [Required, EmailAddress, StringLength(254), Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 12)]
    public string Password { get; set; } = string.Empty;

    [Required, Compare(nameof(Password), ErrorMessage = "Passwords must match."), Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
