using System.Linq.Expressions;
using LocalEats.Application.Basket;
using LocalEats.Application.Checkout;
using LocalEats.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LocalEats.Infrastructure.Persistence;

public sealed class BasketProductReader(LocalEatsDbContext db) : IBasketProductReader, ICheckoutCatalogReader
{
    private IQueryable<MenuItem> PublicItems => db.MenuItems.AsNoTracking().Where(item => item.MenuCategory.Restaurant.IsApproved
        && item.MenuCategory.Restaurant.IsActive && item.MenuCategory.Restaurant.DeletedAt == null);
    private static readonly Expression<Func<MenuItem, BasketProduct>> Projection = item => new BasketProduct(item.Id,
        item.MenuCategory.RestaurantId, item.MenuCategory.Restaurant.Name, item.Name, item.PriceMinor,
        item.IsAvailable && item.MenuCategory.IsActive && item.MenuCategory.Restaurant.IsOpen);

    public Task<BasketProduct?> GetAsync(Guid itemId, CancellationToken cancellationToken = default)
        => PublicItems.Where(item => item.Id == itemId).Select(Projection).SingleOrDefaultAsync(cancellationToken);
    public async Task<IReadOnlyList<BasketProduct>> GetManyAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken = default)
        => await PublicItems.Where(item => ids.Contains(item.Id)).Select(Projection).ToListAsync(cancellationToken);
}
