namespace LocalEats.Application.Users;

public interface IRegistrationStore
{
    Task<RegistrationResult> CreateCustomerAsync(string email, string password, CancellationToken cancellationToken);
}
