using System.ComponentModel.DataAnnotations;

namespace LocalEats.Application.Users;

public sealed class RegistrationService(IRegistrationStore store) : IRegistrationService
{
    public async Task<RegistrationResult> RegisterAsync(
        RegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedRequest = new RegistrationRequest
        {
            Email = request.Email?.Trim() ?? string.Empty,
            Password = request.Password,
            ConfirmPassword = request.ConfirmPassword
        };
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(normalizedRequest, new ValidationContext(normalizedRequest), validationResults, true))
        {
            var errors = validationResults
                .SelectMany(result => result.MemberNames.DefaultIfEmpty(string.Empty)
                    .Select(field => (Field: field, Message: result.ErrorMessage ?? "Check this value.")))
                .GroupBy(error => error.Field)
                .ToDictionary(group => group.Key, group => group.Select(error => error.Message).ToArray());
            return new RegistrationResult(false, errors);
        }

        return await store.CreateCustomerAsync(normalizedRequest.Email, normalizedRequest.Password, cancellationToken);
    }
}
