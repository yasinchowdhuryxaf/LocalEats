using System.ComponentModel.DataAnnotations;

namespace LocalEats.Application.Users;

public sealed class LoginRequest
{
    [Required, EmailAddress, StringLength(254), Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(128)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
