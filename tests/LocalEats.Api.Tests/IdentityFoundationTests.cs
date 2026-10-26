using LocalEats.Infrastructure;
using LocalEats.Infrastructure.Identity;
using LocalEats.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LocalEats.Api.Tests;

public sealed class IdentityFoundationTests
{
    [Fact]
    public void IdentityRegistration_Configured_UsesEfStoresAndSafeDefaults()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=unused;Username=unused"
        }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration);
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>());
        var options = provider.GetRequiredService<IOptions<IdentityOptions>>().Value;
        Assert.True(options.User.RequireUniqueEmail);
        Assert.Equal(12, options.Password.RequiredLength);
        Assert.Equal(5, options.Lockout.MaxFailedAccessAttempts);
    }

    [Fact]
    public void IdentityModel_Configured_MapsUsersAndRolesWithGuidKeys()
    {
        var options = new DbContextOptionsBuilder<LocalEatsDbContext>()
            .UseNpgsql("Host=localhost;Database=unused;Username=unused").Options;
        using var context = new LocalEatsDbContext(options);

        Assert.Equal("AspNetUsers", context.Model.FindEntityType(typeof(ApplicationUser))?.GetTableName());
        Assert.Equal("AspNetRoles", context.Model.FindEntityType(typeof(IdentityRole<Guid>))?.GetTableName());
        Assert.Equal(typeof(Guid), context.Model.FindEntityType(typeof(ApplicationUser))?.FindProperty("Id")?.ClrType);
    }

    [Fact]
    public void IdentityPasswordHasher_HashedPassword_VerifiesOnlyCorrectPassword()
    {
        var user = new ApplicationUser();
        var hasher = new PasswordHasher<ApplicationUser>();
        const string password = "Test-only-Password123!";

        var hash = hasher.HashPassword(user, password);

        Assert.NotEqual(password, hash);
        Assert.Equal(PasswordVerificationResult.Success, hasher.VerifyHashedPassword(user, hash, password));
        Assert.Equal(PasswordVerificationResult.Failed, hasher.VerifyHashedPassword(user, hash, "wrong"));
    }
}
