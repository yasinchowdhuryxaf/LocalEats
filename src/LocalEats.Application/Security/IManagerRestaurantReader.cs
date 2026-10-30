namespace LocalEats.Application.Security;

public interface IManagerRestaurantReader
{
    Task<bool> IsAssignedAsync(Guid managerId, Guid restaurantId, CancellationToken cancellationToken = default);
}
