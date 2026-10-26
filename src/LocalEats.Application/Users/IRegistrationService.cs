namespace LocalEats.Application.Users;

public interface IRegistrationService
{
    Task<RegistrationResult> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default);
}
