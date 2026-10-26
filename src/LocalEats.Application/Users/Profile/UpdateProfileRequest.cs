using System.ComponentModel.DataAnnotations;

namespace LocalEats.Application.Users.Profile;

public sealed class UpdateProfileRequest
{
    [StringLength(100), Display(Name = "Name")]
    public string DisplayName { get; set; } = string.Empty;

    [OptionalPhone, StringLength(32), Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Reload your profile before saving."), StringLength(128, ErrorMessage = "Reload your profile before saving.")]
    public string Version { get; set; } = string.Empty;
}
