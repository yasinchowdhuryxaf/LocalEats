using LocalEats.Application.Users;
using LocalEats.Infrastructure;
using LocalEats.Infrastructure.Identity;
using LocalEats.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LocalEats.Api.Tests;

[Collection("PostgreSQL")]
public sealed class RegistrationPersistenceTests : IAsyncLifetime
{
    private ServiceProvider? provider;
    private readonly List<string> testEmails = [];

    public async Task InitializeAsync()
    {
        var connection = Environment.GetEnvironmentVariable("LOCALEATS_TEST_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=localeats_registration_tests;Username=localeats;Password=localeats_dev_password";
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = connection
        }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration);
        services.AddScoped<IRegistrationService, RegistrationService>();
        provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>();
        if (!database.Database.GetDbConnection().Database.EndsWith("_tests", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Registration tests require a separate database ending in _tests.");
        }

        await database.Database.MigrateAsync();
        await DevelopmentRoleSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>());
    }

    [Fact]
    public async Task RegisterAsync_ValidAccount_PersistsHashAndOnlyCustomerRole()
    {
        var email = NewEmail();
        var result = await Register(email);
        Assert.True(result.Succeeded);
        await using var scope = Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByEmailAsync(email);
        Assert.NotNull(user);
        Assert.True(await users.CheckPasswordAsync(user, "Valid-Password123!"));
        Assert.NotEqual("Valid-Password123!", user.PasswordHash);
        Assert.Equal(new[] { ApplicationRoles.Customer }, await users.GetRolesAsync(user));
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmailIgnoringCase_ReturnsSafeFailure()
    {
        var email = NewEmail();
        Assert.True((await Register(email)).Succeeded);
        var duplicate = await Register(email.ToUpperInvariant());
        Assert.False(duplicate.Succeeded);
        Assert.Contains("Unable to create an account", duplicate.Errors[string.Empty][0]);
        await AssertUserCount(email, 1);
    }

    [Fact]
    public async Task RegisterAsync_WeakIdentityPassword_PersistsNoUser()
    {
        var email = NewEmail();
        var result = await Register(email, "onlylowercaseletters");
        Assert.False(result.Succeeded);
        Assert.Contains("Password", result.Errors.Keys);
        await AssertUserCount(email, 0);
    }

    [Fact]
    public async Task RegisterAsync_ConcurrentDuplicate_CreatesExactlyOneCustomer()
    {
        var email = NewEmail();
        var results = await Task.WhenAll(Register(email), Register(email));
        Assert.Single(results, result => result.Succeeded);
        await AssertUserCount(email, 1);
    }

    private ServiceProvider Services => provider ?? throw new InvalidOperationException("Tests are not initialized.");

    private string NewEmail()
    {
        var email = $"registration-{Guid.NewGuid():N}@example.test";
        testEmails.Add(email);
        return email;
    }

    private async Task<RegistrationResult> Register(string email, string password = "Valid-Password123!")
    {
        await using var scope = Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IRegistrationService>().RegisterAsync(new RegistrationRequest
        {
            Email = email,
            Password = password,
            ConfirmPassword = password
        });
    }

    private async Task AssertUserCount(string email, int expected)
    {
        await using var scope = Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>();
        Assert.Equal(expected, await database.Users.CountAsync(user => user.NormalizedEmail == email.ToUpperInvariant()));
    }

    public async Task DisposeAsync()
    {
        if (provider is null)
        {
            return;
        }

        await using (var scope = provider.CreateAsyncScope())
        {
            var database = scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>();
            foreach (var email in testEmails)
            {
                await database.Users.Where(user => user.NormalizedEmail == email.ToUpperInvariant()).ExecuteDeleteAsync();
            }
        }

        await provider.DisposeAsync();
    }
}
