namespace LocalEats.Application.Orders;

public interface IOrderDetailsReader
{
    Task<OrderDetails?> GetAsync(Guid orderId, Guid customerId, bool isAdmin, int page, int pageSize, CancellationToken cancellationToken = default);
}
