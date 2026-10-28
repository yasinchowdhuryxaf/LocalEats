namespace LocalEats.Application.Restaurants;

public interface IRestaurantDirectoryReader
{
    Task<RestaurantDirectoryResult> SearchAsync(RestaurantDirectoryQuery query, int pageSize, CancellationToken cancellationToken = default);
}
