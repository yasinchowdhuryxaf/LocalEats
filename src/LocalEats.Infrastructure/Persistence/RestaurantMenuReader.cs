using LocalEats.Application.Restaurants;
using Microsoft.EntityFrameworkCore;

namespace LocalEats.Infrastructure.Persistence;

public sealed class RestaurantMenuReader(LocalEatsDbContext db) : IRestaurantMenuReader
{
    public async Task<RestaurantMenuPage?> GetAsync(string slug, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var restaurant = await db.Restaurants.AsNoTracking()
            .Where(row => row.Slug == slug && row.IsApproved && row.IsActive && row.DeletedAt == null)
            .Select(row => new { row.Id, row.Name, row.Description, row.Address, row.Area, row.Cuisine, row.IsOpen })
            .SingleOrDefaultAsync(cancellationToken);
        if (restaurant is null) return null;
        var query = db.MenuItems.AsNoTracking().Where(item => item.MenuCategory.RestaurantId == restaurant.Id && item.MenuCategory.IsActive);
        var total = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Max(1, ((long)total + pageSize - 1) / pageSize);
        page = Math.Clamp(page, 1, totalPages);
        var items = await query.OrderBy(item => item.MenuCategory.SortOrder).ThenBy(item => item.MenuCategory.Name)
            .ThenBy(item => item.MenuCategoryId).ThenBy(item => item.Name).ThenBy(item => item.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(item => new MenuEntry(item.Id, item.MenuCategoryId, item.MenuCategory.Name, item.Name, item.Description, item.PriceMinor, item.IsAvailable))
            .ToListAsync(cancellationToken);
        return new RestaurantMenuPage(restaurant.Name, restaurant.Description, restaurant.Address, restaurant.Area, restaurant.Cuisine,
            restaurant.IsOpen, items, total, page, pageSize);
    }
}
