namespace LocalEats.Application.Users.Profile;

public sealed record ProfileUpdateResult(bool Succeeded, IReadOnlyDictionary<string, string[]> Errors)
{
    public static ProfileUpdateResult Success() => new(true, new Dictionary<string, string[]>());

    public static ProfileUpdateResult Failure(string message) =>
        new(false, new Dictionary<string, string[]> { [string.Empty] = [message] });
}
