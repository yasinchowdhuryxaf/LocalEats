using System.ComponentModel.DataAnnotations;

namespace LocalEats.Application.Users.Profile;

[AttributeUsage(AttributeTargets.Property)]
public sealed class OptionalPhoneAttribute : ValidationAttribute
{
    public OptionalPhoneAttribute() => ErrorMessage = "Enter a valid phone number or leave it blank.";

    public override bool IsValid(object? value) => value is null
        || value is string text && (string.IsNullOrWhiteSpace(text) || new PhoneAttribute().IsValid(text.Trim()));
}
