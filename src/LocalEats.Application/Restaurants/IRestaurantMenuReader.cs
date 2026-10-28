namespace LocalEats.Application.Restaurants;

public interface IRestaurantMenuReader
{
    Task<RestaurantMenuPage?> GetAsync(string slug, int page, int pageSize, CancellationToken cancellationToken = default);
}
