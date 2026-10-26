using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace LocalEats.Application.Users.Profile;

public sealed class ProfileService(IProfileStore profiles)
{
    public Task<ProfileSnapshot?> GetAsync(ClaimsPrincipal user) => TryGetUserId(user, out var id)
        ? profiles.GetAsync(id)
        : Task.FromResult<ProfileSnapshot?>(null);

    public async Task<ProfileUpdateResult> UpdateAsync(ClaimsPrincipal user, UpdateProfileRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!TryGetUserId(user, out var userId))
        {
            return ProfileUpdateResult.Failure("Unable to save your profile. Please sign in again.");
        }

        var input = new UpdateProfileRequest
        {
            DisplayName = request.DisplayName?.Trim() ?? string.Empty,
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            Version = request.Version
        };
        var validation = new List<ValidationResult>();
        if (!Validator.TryValidateObject(input, new ValidationContext(input), validation, true))
        {
            var errors = validation.SelectMany(result => result.MemberNames.DefaultIfEmpty(string.Empty)
                    .Select(field => (Field: field, Message: result.ErrorMessage ?? "Check this value.")))
                .GroupBy(error => error.Field)
                .ToDictionary(group => group.Key, group => group.Select(error => error.Message).ToArray());
            return new ProfileUpdateResult(false, errors);
        }

        return await profiles.UpdateAsync(userId, input);
    }

    private static bool TryGetUserId(ClaimsPrincipal user, out Guid id)
    {
        id = Guid.Empty;
        return user.Identity?.IsAuthenticated == true
            && Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out id)
            && id != Guid.Empty;
    }
}
