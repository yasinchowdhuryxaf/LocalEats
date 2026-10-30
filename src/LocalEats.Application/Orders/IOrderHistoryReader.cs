namespace LocalEats.Application.Orders;

public interface IOrderHistoryReader
{
    Task<OrderHistoryPage> GetAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default);
}
