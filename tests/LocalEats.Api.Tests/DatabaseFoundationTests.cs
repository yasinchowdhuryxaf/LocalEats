using LocalEats.Infrastructure;
using LocalEats.Infrastructure.Persistence.Seed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LocalEats.Api.Tests;

public sealed class DatabaseFoundationTests
{
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
