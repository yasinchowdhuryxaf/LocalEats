using LocalEats.Infrastructure;
using LocalEats.Infrastructure.Persistence;
using LocalEats.Infrastructure.Persistence.Seed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LocalEats.Api.Tests;

[Collection("PostgreSQL")]
public sealed class DatabaseFoundationTests
{
    [Fact]
    public void EfTools_ExplicitLocalEatsEndpoint_CreatesContextWithoutConnecting()
    {
        var previous = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        try
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Host=127.0.0.1;Port=15432;Database=localeats");
            using var context = new LocalEatsDbContextFactory().CreateDbContext([]);
            Assert.NotNull(context);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", previous);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Host=localhost;Port=5432;Database=localeats")]
    [InlineData("Host=127.0.0.1;Port=15432;Database=another_project")]
    public void EfTools_MissingOrUnapprovedEndpoint_RejectsBeforeConnecting(string? connection)
    {
        var previous = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        try
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", connection);
            Assert.Throws<InvalidOperationException>(() => new LocalEatsDbContextFactory().CreateDbContext([]));
        }
        finally
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", previous);
        }
    }

    [Fact]
    public void AddInfrastructure_WithoutConnectionString_FailsClearly()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddInfrastructure(configuration));

        Assert.Contains("ConnectionStrings:DefaultConnection", exception.Message);
    }

    [Fact]
    public async Task SeedDevelopmentData_OutsideDevelopment_DoesNotResolveDatabase()
    {
        await using var services = new ServiceCollection().BuildServiceProvider();

        await services.SeedDevelopmentDataAsync(isDevelopment: false);
    }
}
