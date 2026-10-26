namespace LocalEats.Application.Users;

public sealed record RegistrationResult(bool Succeeded, IReadOnlyDictionary<string, string[]> Errors)
{
    public static RegistrationResult Success() => new(true, new Dictionary<string, string[]>());

    public static RegistrationResult Failure(string field, string message) =>
        new(false, new Dictionary<string, string[]> { [field] = [message] });
}
