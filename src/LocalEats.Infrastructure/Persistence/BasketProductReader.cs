using LocalEats.Application.Basket;
using Microsoft.EntityFrameworkCore;

namespace LocalEats.Infrastructure.Persistence;

public sealed class BasketProductReader(LocalEatsDbContext db) : IBasketProductReader
{
    public Task<BasketProduct?> GetAsync(Guid itemId, CancellationToken cancellationToken = default)
        => db.MenuItems.AsNoTracking().Where(item => item.Id == itemId && item.MenuCategory.Restaurant.IsApproved
            && item.MenuCategory.Restaurant.IsActive && item.MenuCategory.Restaurant.DeletedAt == null)
            .Select(item => new BasketProduct(item.Id, item.MenuCategory.RestaurantId, item.MenuCategory.Restaurant.Name,
                item.Name, item.PriceMinor, item.IsAvailable && item.MenuCategory.IsActive && item.MenuCategory.Restaurant.IsOpen))
            .SingleOrDefaultAsync(cancellationToken);
}
