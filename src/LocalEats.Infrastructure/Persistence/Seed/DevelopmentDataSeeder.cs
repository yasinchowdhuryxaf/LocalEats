using LocalEats.Domain.Entities;
using LocalEats.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LocalEats.Infrastructure.Persistence.Seed;

public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        await DevelopmentRoleSeeder.SeedAsync(roleManager);

        var now = DateTimeOffset.UtcNow;
        var burgerCategory = new MenuCategory
        {
            Id = Guid.NewGuid(),
            Name = "Burgers",
            SortOrder = 1,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        burgerCategory.Items.Add(new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = "Classic Beef Burger",
            Description = "Beef patty, lettuce, tomato, and house sauce.",
            PriceMinor = 39900,
            IsAvailable = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        var riceCategory = new MenuCategory
        {
            Id = Guid.NewGuid(),
            Name = "Rice Bowls",
            SortOrder = 1,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        riceCategory.Items.Add(new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = "Chicken Rice Bowl",
            Description = "Spiced chicken, fragrant rice, and fresh salad.",
            PriceMinor = 32900,
            IsAvailable = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        var restaurants = new[]
        {
            new Restaurant
            {
                Id = Guid.NewGuid(),
                Name = "Dhaka Bites",
                Slug = "dhaka-bites",
                Description = "Comfort food made for busy Dhaka days.",
                Address = "Demo address, Dhanmondi, Dhaka (sample only)",
                Area = "Dhanmondi",
                Cuisine = "Burgers",
                IsApproved = true,
                IsActive = true,
                IsOpen = true,
                CreatedAt = now,
                UpdatedAt = now,
                Categories = new List<MenuCategory> { burgerCategory }
            },
            new Restaurant
            {
                Id = Guid.NewGuid(),
                Name = "Bowl House",
                Slug = "bowl-house",
                Description = "Simple rice bowls with bold local flavours.",
                Address = "Demo address, Uttara, Dhaka (sample only)",
                Area = "Uttara",
                Cuisine = "Bangladeshi",
                IsApproved = true,
                IsActive = true,
                IsOpen = false,
                CreatedAt = now,
                UpdatedAt = now,
                Categories = new List<MenuCategory> { riceCategory }
            }
        };

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        // Serialize only LocalEats sample inserts when both local hosts start together.
        await dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(21875901)", cancellationToken);
        var sampleSlugs = restaurants.Select(restaurant => restaurant.Slug).ToArray();
        var existingSlugs = await dbContext.Restaurants.AsNoTracking()
            .Where(restaurant => sampleSlugs.Contains(restaurant.Slug))
            .Select(restaurant => restaurant.Slug).ToListAsync(cancellationToken);
        dbContext.Restaurants.AddRange(restaurants.Where(restaurant => !existingSlugs.Contains(restaurant.Slug)));
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
