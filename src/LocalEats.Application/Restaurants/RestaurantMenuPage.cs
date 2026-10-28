namespace LocalEats.Application.Restaurants;

public sealed record RestaurantMenuPage(string Name, string? Description, string? Address, string? Area, string? Cuisine,
    bool IsOpen, IReadOnlyList<MenuEntry> Items, int Total, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Max(1, ((long)Total + PageSize - 1) / PageSize);
}
