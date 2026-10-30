using LocalEats.Application.Orders;
using Microsoft.EntityFrameworkCore;

namespace LocalEats.Infrastructure.Persistence;

public sealed class OrderHistoryReader(LocalEatsDbContext db) : IOrderHistoryReader
{
    public async Task<OrderHistoryPage> GetAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var own = db.Orders.AsNoTracking().Where(order => order.CustomerId == customerId);
        var count = await own.CountAsync(cancellationToken);
        var totalPages = count == 0 ? 1 : ((count - 1) / pageSize) + 1;
        page = Math.Clamp(page, 1, totalPages);
        var items = await own.OrderByDescending(order => order.CreatedAt).ThenByDescending(order => order.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(order => new OrderHistoryEntry(order.Id, order.OrderNumber, order.Restaurant.Name, order.CreatedAt,
                order.TotalMinor, order.Currency, order.Status.ToString()))
            .ToListAsync(cancellationToken);
        return new(items, count, page, totalPages);
    }
}
