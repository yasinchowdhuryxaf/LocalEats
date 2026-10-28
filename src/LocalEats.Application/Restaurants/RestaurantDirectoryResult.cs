namespace LocalEats.Application.Restaurants;

public sealed record RestaurantDirectoryResult(IReadOnlyList<RestaurantCard> Items, int Total, int Page, int PageSize, string? Error = null)
{
    public int TotalPages => (int)Math.Max(1, ((long)Total + PageSize - 1) / PageSize);
}
