using LocalEats.Domain.Entities;
using LocalEats.Infrastructure;
using LocalEats.Infrastructure.Persistence;
using LocalEats.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LocalEats.Api.Tests;

[Collection("PostgreSQL")]
public sealed class DevelopmentSeedTests : IAsyncLifetime
{
    private readonly string databaseName = $"localeats_seed_{Guid.NewGuid():N}_tests";
    private string adminConnection = string.Empty;
    private string connection = string.Empty;

    public async Task InitializeAsync()
    {
        adminConnection = Environment.GetEnvironmentVariable("LOCALEATS_TEST_CONNECTION")
            ?? "Host=127.0.0.1;Port=15432;Database=localeats_registration_tests;Username=localeats;Password=localeats_dev_password";
        var target = new Npgsql.NpgsqlConnectionStringBuilder(adminConnection);
        if (target.Database?.EndsWith("_tests", StringComparison.Ordinal) != true)
            throw new InvalidOperationException("Seed tests require a dedicated _tests database.");
        await using var admin = new Npgsql.NpgsqlConnection(adminConnection);
        await admin.OpenAsync();
        await using var command = new Npgsql.NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", admin);
        await command.ExecuteNonQueryAsync();
        target.Database = databaseName;
        connection = target.ConnectionString;
    }

    public async Task DisposeAsync()
    {
        if (string.IsNullOrEmpty(connection)) return;
        using var pool = new Npgsql.NpgsqlConnection(connection);
        Npgsql.NpgsqlConnection.ClearPool(pool);
        await using var admin = new Npgsql.NpgsqlConnection(adminConnection);
        await admin.OpenAsync();
        await using var command = new Npgsql.NpgsqlCommand($"DROP DATABASE \"{databaseName}\" WITH (FORCE)", admin);
        await command.ExecuteNonQueryAsync();
    }

    [Fact]
    public async Task Seed_ExistingDataAndRepeatRun_AddsSamplesWithoutOverwriting()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = connection
        }).Build();
        var registrations = new ServiceCollection();
        registrations.AddLogging();
        registrations.AddInfrastructure(configuration);
        await using var services = registrations.BuildServiceProvider();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>();
        await db.Database.MigrateAsync();
        string[] slugs = ["dhaka-bites", "bowl-house"];
        Assert.False(await db.Restaurants.AnyAsync(row => slugs.Contains(row.Slug)), "Test sample slugs must be unowned before this fixture runs.");
        var ownedIds = new List<Guid>();
        var existing = new Restaurant
        {
            Id = Guid.NewGuid(),
            Name = "Existing fixture restaurant",
            Slug = $"seed-probe-{Guid.NewGuid():N}",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        try
        {
            db.Restaurants.Add(existing);
            await db.SaveChangesAsync();
            ownedIds.Add(existing.Id);
            await services.SeedDevelopmentDataAsync(true);
            var samples = await db.Restaurants.AsNoTracking().Include(row => row.Categories).ThenInclude(category => category.Items)
                .Where(row => slugs.Contains(row.Slug)).OrderBy(row => row.Slug).ToListAsync();
            ownedIds.AddRange(samples.Select(row => row.Id));
            Assert.Equal(2, samples.Count);
            Assert.All(samples, restaurant =>
            {
                Assert.True(restaurant.IsVisible);
                Assert.False(string.IsNullOrWhiteSpace(restaurant.Address));
                Assert.False(string.IsNullOrWhiteSpace(restaurant.Area));
                Assert.False(string.IsNullOrWhiteSpace(restaurant.Cuisine));
                var category = Assert.Single(restaurant.Categories);
                Assert.True(category.IsActive);
                var item = Assert.Single(category.Items);
                Assert.True(item.IsAvailable);
                Assert.True(item.PriceMinor > 0);
            });
            Assert.True(samples.Single(row => row.Slug == "dhaka-bites").IsOpen);
            Assert.False(samples.Single(row => row.Slug == "bowl-house").IsOpen);
            var edited = await db.Restaurants.SingleAsync(row => row.Slug == "dhaka-bites");
            edited.Name = "Developer edited sample";
            await db.SaveChangesAsync();
            await services.SeedDevelopmentDataAsync(true);
            db.ChangeTracker.Clear();
            var repeated = await db.Restaurants.AsNoTracking().Where(row => slugs.Contains(row.Slug)).ToListAsync();
            Assert.Equal(samples.Select(row => row.Id).Order(), repeated.Select(row => row.Id).Order());
            Assert.Equal("Developer edited sample", repeated.Single(row => row.Slug == "dhaka-bites").Name);
            Assert.Equal(2, await db.MenuCategories.CountAsync(row => ownedIds.Contains(row.RestaurantId)));
            Assert.Equal(2, await db.MenuItems.CountAsync(row => ownedIds.Contains(row.MenuCategory.RestaurantId)));
            Assert.Equal("Existing fixture restaurant", (await db.Restaurants.SingleAsync(row => row.Id == existing.Id)).Name);
        }
        finally
        {
            await db.Restaurants.Where(row => ownedIds.Contains(row.Id)).ExecuteDeleteAsync();
        }
    }
}
