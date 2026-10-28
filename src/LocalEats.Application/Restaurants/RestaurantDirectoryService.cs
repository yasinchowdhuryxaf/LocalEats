namespace LocalEats.Application.Restaurants;

public sealed class RestaurantDirectoryService(IRestaurantDirectoryReader reader)
{
    public const int PageSize = 6;

    public Task<RestaurantDirectoryResult> SearchAsync(RestaurantDirectoryQuery query, CancellationToken cancellationToken = default)
    {
        var normalized = new RestaurantDirectoryQuery(query.Search?.Trim(), query.Area?.Trim(), query.Cuisine?.Trim(), Math.Max(1, query.Page));
        if (normalized.Search?.Length > 160 || normalized.Area?.Length > 120 || normalized.Cuisine?.Length > 120)
            return Task.FromResult(new RestaurantDirectoryResult([], 0, 1, PageSize, "Use up to 160 characters for name and 120 for area or cuisine."));
        return reader.SearchAsync(normalized, PageSize, cancellationToken);
    }
}
