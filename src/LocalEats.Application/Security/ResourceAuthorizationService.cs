using System.Security.Claims;

namespace LocalEats.Application.Security;

public sealed class ResourceAuthorizationService(IOrderOwnershipReader orders, IManagerRestaurantReader assignments) : IResourceAuthorizationService
{
    public Task<bool> CanManageRestaurantAsync(ClaimsPrincipal user, Guid restaurantId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (user.Identity?.IsAuthenticated != true || !user.IsInRole(RoleNames.Manager)
            || restaurantId == Guid.Empty || !Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) || id == Guid.Empty)
            return Task.FromResult(false);
        return assignments.IsAssignedAsync(id, restaurantId, cancellationToken);
    }

    public async Task<bool> CanReadOrderAsync(
        ClaimsPrincipal user,
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (user.Identity?.IsAuthenticated != true
            || orderId == Guid.Empty
            || !Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId)
            || userId == Guid.Empty)
        {
            return false;
        }

        var isAdmin = user.IsInRole(RoleNames.Admin);
        var isManager = user.IsInRole(RoleNames.Manager);
        if (!isAdmin && !isManager && !user.IsInRole(RoleNames.Customer))
        {
            return false;
        }

        var order = await orders.FindAsync(orderId, cancellationToken);
        return order is not null && (isAdmin || (user.IsInRole(RoleNames.Customer) && order.CustomerId == userId)
            || (isManager && await assignments.IsAssignedAsync(userId, order.RestaurantId, cancellationToken)));
    }
}
