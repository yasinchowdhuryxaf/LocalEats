namespace LocalEats.Application.Restaurants;

public sealed class RestaurantMenuService(IRestaurantMenuReader reader)
{
    public const int PageSize = 20;

    public Task<RestaurantMenuPage?> GetAsync(string? slug, int page = 1, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug) || slug.Length > 160 || slug.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '-'))
            return Task.FromResult<RestaurantMenuPage?>(null);
        return reader.GetAsync(slug.ToLowerInvariant(), Math.Max(1, page), PageSize, cancellationToken);
    }
}
