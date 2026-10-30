using System.Security.Claims;
using LocalEats.Application.Security;

namespace LocalEats.Application.Orders;

public sealed class OrderDetailsService(IResourceAuthorizationService authorization, IOrderDetailsReader reader)
{
    public const int PageSize = 20;

    public async Task<OrderDetails?> GetAsync(ClaimsPrincipal user, string? id, int page = 1, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(id, out var orderId) || orderId == Guid.Empty
            || user.Identity?.IsAuthenticated != true
            || !Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId)
            || userId == Guid.Empty
            || !await authorization.CanReadOrderAsync(user, orderId, cancellationToken)) return null;

        return await reader.GetAsync(orderId, userId, user.IsInRole(RoleNames.Admin), user.IsInRole(RoleNames.Customer),
            user.IsInRole(RoleNames.Manager), Math.Max(1, page), PageSize, cancellationToken);
    }
}
