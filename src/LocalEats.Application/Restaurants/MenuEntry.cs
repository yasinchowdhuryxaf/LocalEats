namespace LocalEats.Application.Restaurants;

public sealed record MenuEntry(Guid CategoryId, string Category, string Name, string? Description, long PriceMinor, bool IsAvailable);
