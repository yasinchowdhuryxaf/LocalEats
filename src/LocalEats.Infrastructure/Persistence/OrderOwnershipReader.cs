using LocalEats.Application.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LocalEats.Infrastructure.Persistence;

public sealed class OrderOwnershipReader(IServiceScopeFactory scopes) : IOrderOwnershipReader
{
    public async Task<OrderOwnership?> FindAsync(Guid orderId, CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>();
        return await database.Orders.AsNoTracking()
            .Where(order => order.Id == orderId)
            .Select(order => new OrderOwnership(order.Id, order.CustomerId, order.RestaurantId))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
