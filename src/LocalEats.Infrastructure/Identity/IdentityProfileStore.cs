using LocalEats.Application.Users.Profile;
using LocalEats.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace LocalEats.Infrastructure.Identity;

public sealed class IdentityProfileStore(
    UserManager<ApplicationUser> users,
    SignInManager<ApplicationUser> signIn,
    LocalEatsDbContext database,
    ILogger<IdentityProfileStore> logger) : IProfileStore
{
    public async Task<ProfileSnapshot?> GetAsync(Guid userId)
    {
        try
        {
            var user = await users.FindByIdAsync(userId.ToString());
            return user is null ? null : new ProfileSnapshot(
                user.Email ?? string.Empty, user.DisplayName, user.PhoneNumber, user.ConcurrencyStamp ?? string.Empty);
        }
        catch (Exception exception)
        {
            logger.LogError("Profile read failed with error type {ErrorType}.", exception.GetType().Name);
            return null;
        }
    }

    public async Task<ProfileUpdateResult> UpdateAsync(Guid userId, UpdateProfileRequest request)
    {
        try
        {
            var user = await users.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                return ProfileUpdateResult.Failure("Unable to save your profile. Please sign in again.");
            }

            if (!string.Equals(user.ConcurrencyStamp, request.Version, StringComparison.Ordinal))
            {
                return ProfileUpdateResult.Failure("Your profile changed in another session. Reload before saving.");
            }

            await using var transaction = await database.Database.BeginTransactionAsync();
            user.DisplayName = request.DisplayName;
            var phoneChanged = !string.Equals(user.PhoneNumber, request.PhoneNumber, StringComparison.Ordinal);
            var result = phoneChanged
                ? await users.SetPhoneNumberAsync(user, request.PhoneNumber)
                : await users.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return ProfileUpdateResult.Failure("Unable to save your profile. Reload the page and try again.");
            }

            await transaction.CommitAsync();
            if (phoneChanged)
            {
                // Identity rotates the security stamp for phone edits; keep this browser signed in.
                await signIn.RefreshSignInAsync(user);
            }

            return ProfileUpdateResult.Success();
        }
        catch (Exception exception)
        {
            logger.LogError("Profile update failed with error type {ErrorType}.", exception.GetType().Name);
            return ProfileUpdateResult.Failure("Unable to save your profile right now. Please try again later.");
        }
    }
}
