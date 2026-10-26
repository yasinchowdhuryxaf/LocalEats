using System.Security.Claims;

namespace LocalEats.Application.Security;

public sealed class ResourceAuthorizationService(IOrderOwnershipReader orders) : IResourceAuthorizationService
{
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
        if (!isAdmin && !user.IsInRole(RoleNames.Customer))
        {
            // Manager access requires the verified restaurant assignment introduced in TASK-025.
            return false;
        }

        var order = await orders.FindAsync(orderId, cancellationToken);
        return order is not null && (isAdmin || order.CustomerId == userId);
    }
}
