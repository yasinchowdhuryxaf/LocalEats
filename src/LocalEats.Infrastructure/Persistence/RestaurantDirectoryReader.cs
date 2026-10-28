using LocalEats.Application.Restaurants;
using Microsoft.EntityFrameworkCore;

namespace LocalEats.Infrastructure.Persistence;

public sealed class RestaurantDirectoryReader(LocalEatsDbContext db) : IRestaurantDirectoryReader
{
    public async Task<RestaurantDirectoryResult> SearchAsync(RestaurantDirectoryQuery query, int pageSize, CancellationToken cancellationToken = default)
    {
        var rows = db.Restaurants.AsNoTracking().Where(row => row.IsApproved && row.IsActive && row.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLowerInvariant();
            rows = rows.Where(row => row.Name.ToLower().Contains(search));
        }
        if (!string.IsNullOrWhiteSpace(query.Area))
        {
            var area = query.Area.ToLowerInvariant();
            rows = rows.Where(row => row.Area != null && row.Area.ToLower() == area);
        }
        if (!string.IsNullOrWhiteSpace(query.Cuisine))
        {
            var cuisine = query.Cuisine.ToLowerInvariant();
            rows = rows.Where(row => row.Cuisine != null && row.Cuisine.ToLower() == cuisine);
        }
        var total = await rows.CountAsync(cancellationToken);
        var pages = (int)Math.Max(1, ((long)total + pageSize - 1) / pageSize);
        var page = Math.Clamp(query.Page, 1, pages);
        var items = await rows.OrderBy(row => row.Name).ThenBy(row => row.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(row => new RestaurantCard(row.Id, row.Name, row.Description, row.Area, row.Cuisine, row.IsOpen, row.Slug))
            .ToListAsync(cancellationToken);
        return new RestaurantDirectoryResult(items, total, page, pageSize);
    }
}
