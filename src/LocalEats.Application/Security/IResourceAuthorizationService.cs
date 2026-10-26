using System.Security.Claims;

namespace LocalEats.Application.Security;

public interface IResourceAuthorizationService
{
    Task<bool> CanReadOrderAsync(ClaimsPrincipal user, Guid orderId, CancellationToken cancellationToken = default);
}
