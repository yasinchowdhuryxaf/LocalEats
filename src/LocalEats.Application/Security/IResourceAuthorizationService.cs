using System.Security.Claims;

namespace LocalEats.Application.Security;

public interface IResourceAuthorizationService
{
    Task<bool> CanManageRestaurantAsync(ClaimsPrincipal user, Guid restaurantId, CancellationToken cancellationToken = default);
    Task<bool> CanReadOrderAsync(ClaimsPrincipal user, Guid orderId, CancellationToken cancellationToken = default);
}
