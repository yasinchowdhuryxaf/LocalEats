using LocalEats.Application.Users;
using LocalEats.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace LocalEats.Infrastructure.Identity;

public sealed class IdentityRegistrationStore(
    IServiceScopeFactory scopeFactory,
    ILogger<IdentityRegistrationStore> logger) : IRegistrationStore
{
    private const string AccountUnavailable = "Unable to create an account with those details. If you already have an account, sign in.";

    public async Task<RegistrationResult> CreateCustomerAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        // Each operation owns its EF scope; a Blazor circuit must not retain a shared DbContext.
        await using var scope = scopeFactory.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var database = scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!await roles.RoleExistsAsync(ApplicationRoles.Customer))
            {
                logger.LogError("Customer role is missing; registration cannot proceed.");
                return RegistrationResult.Failure(string.Empty, "Registration is temporarily unavailable. Please try again later.");
            }

            await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
            var user = new ApplicationUser { UserName = email, Email = email };
            var created = await users.CreateAsync(user, password);
            if (!created.Succeeded)
            {
                if (created.Errors.Any(error => error.Code is "DuplicateEmail" or "DuplicateUserName"))
                {
                    return RegistrationResult.Failure(string.Empty, AccountUnavailable);
                }

                var passwordErrors = created.Errors.Where(error => error.Code.StartsWith("Password", StringComparison.Ordinal)).ToArray();
                return passwordErrors.Length > 0
                    ? RegistrationResult.Failure(nameof(RegistrationRequest.Password),
                        "Use at least 12 characters with uppercase, lowercase, a number, and a symbol.")
                    : RegistrationResult.Failure(string.Empty, AccountUnavailable);
            }

            var assigned = await users.AddToRoleAsync(user, ApplicationRoles.Customer);
            if (!assigned.Succeeded)
            {
                logger.LogError("Customer role assignment failed during registration.");
                return RegistrationResult.Failure(string.Empty, "Registration is temporarily unavailable. Please try again later.");
            }

            await transaction.CommitAsync(cancellationToken);
            return RegistrationResult.Success();
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "UserNameIndex"
        })
        {
            return RegistrationResult.Failure(string.Empty, AccountUnavailable);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            // Do not log email, passwords, request bodies, or provider error payloads.
            logger.LogError("Registration failed with error type {ErrorType}.", exception.GetType().Name);
            return RegistrationResult.Failure(string.Empty, "Registration is temporarily unavailable. Please try again later.");
        }
    }
}
