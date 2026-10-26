namespace LocalEats.Application.Security;

public interface IOrderOwnershipReader
{
    Task<OrderOwnership?> FindAsync(Guid orderId, CancellationToken cancellationToken);
}
