using LocalEats.Domain.Entities;
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

        if (await dbContext.Restaurants.AnyAsync(cancellationToken))
        {
            return;
        }

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
                Area = "Dhanmondi",
                Cuisine = "Burgers",
                IsApproved = true,
                IsActive = true,
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
                Area = "Uttara",
                Cuisine = "Bangladeshi",
                IsApproved = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                Categories = new List<MenuCategory> { riceCategory }
            }
        };

        dbContext.Restaurants.AddRange(restaurants);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
