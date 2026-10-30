namespace LocalEats.Application.Orders;

public interface IOrderDetailsReader
{
    Task<OrderDetails?> GetAsync(Guid orderId, Guid userId, bool isAdmin, bool isCustomer, bool isManager, int page, int pageSize, CancellationToken cancellationToken = default);
}
