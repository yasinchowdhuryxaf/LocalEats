namespace LocalEats.Application.Users.Profile;

public sealed record ProfileSnapshot(string Email, string DisplayName, string? PhoneNumber, string Version);
