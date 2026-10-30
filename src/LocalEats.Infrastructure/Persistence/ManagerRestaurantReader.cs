using LocalEats.Application.Security;
using Microsoft.EntityFrameworkCore;

namespace LocalEats.Infrastructure.Persistence;

public sealed class ManagerRestaurantReader(LocalEatsDbContext db) : IManagerRestaurantReader
{
    public Task<bool> IsAssignedAsync(Guid managerId, Guid restaurantId, CancellationToken cancellationToken = default)
        => db.ManagerRestaurants.AsNoTracking().AnyAsync(assignment => assignment.ManagerId == managerId
            && assignment.RestaurantId == restaurantId && assignment.Restaurant.DeletedAt == null, cancellationToken);
}
