using LocalEats.Application.Orders;
using Microsoft.EntityFrameworkCore;

namespace LocalEats.Infrastructure.Persistence;

public sealed class OrderDetailsReader(LocalEatsDbContext db) : IOrderDetailsReader
{
    public async Task<OrderDetails?> GetAsync(Guid orderId, Guid customerId, bool isAdmin, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        // Repeat ownership in SQL so full details are never fetched for another customer.
        var allowed = db.Orders.AsNoTracking().Where(order => order.Id == orderId && (isAdmin || order.CustomerId == customerId));
        var header = await allowed.Select(order => new OrderDetails(order.Id, order.OrderNumber, order.Restaurant.Name,
            order.Status.ToString(), order.Currency, order.TotalMinor, order.CreatedAt, order.ContactName, order.ContactPhone,
            order.DeliveryAddress, order.Notes, order.PaymentMethod, new List<OrderDetailLine>(), 1, 1)).SingleOrDefaultAsync(cancellationToken);
        if (header is null) return null;
        var items = allowed.SelectMany(order => order.Items);
        var count = await items.CountAsync(cancellationToken);
        var totalPages = count == 0 ? 1 : ((count - 1) / pageSize) + 1;
        page = Math.Clamp(page, 1, totalPages);
        var lines = await items.OrderBy(item => item.ItemName).ThenBy(item => item.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(item => new OrderDetailLine(item.ItemName, item.Quantity, item.UnitPriceMinor, item.LineTotalMinor))
            .ToListAsync(cancellationToken);
        return header with { Items = lines, Page = page, TotalPages = totalPages };
    }
}
