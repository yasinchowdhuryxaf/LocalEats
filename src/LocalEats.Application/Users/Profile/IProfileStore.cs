namespace LocalEats.Application.Users.Profile;

public interface IProfileStore
{
    Task<ProfileSnapshot?> GetAsync(Guid userId);

    Task<ProfileUpdateResult> UpdateAsync(Guid userId, UpdateProfileRequest request);
}
