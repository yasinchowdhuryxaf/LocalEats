using System.Security.Claims;
using LocalEats.Application.Security;

namespace LocalEats.Application.Orders;

public sealed class OrderHistoryService(IOrderHistoryReader reader)
{
    public const int PageSize = 10;

    public async Task<OrderHistoryPage?> GetAsync(ClaimsPrincipal user, int page = 1, CancellationToken cancellationToken = default)
    {
        if (user.Identity?.IsAuthenticated != true || !user.IsInRole(RoleNames.Customer)
            || !Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var customerId) || customerId == Guid.Empty)
            return null;
        return await reader.GetAsync(customerId, Math.Max(1, page), PageSize, cancellationToken);
    }
}
