using System.Data;
using LocalEats.Application.Basket;
using LocalEats.Application.Checkout;
using LocalEats.Application.Orders;
using LocalEats.Domain.Entities;
using LocalEats.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace LocalEats.Infrastructure.Persistence;

public sealed class OrderPlacementStore(IServiceScopeFactory scopes) : IOrderPlacementStore
{
    private static OrderReceipt Receipt(Order order) => new(order.Id, order.OrderNumber, order.TotalMinor, order.PaymentMethod ?? "Unknown", order.Status.ToString());
    public async Task<OrderReceipt?> FindAsync(Guid customerId, Guid requestId, CancellationToken cancellationToken = default)
    {
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>();
        var order = await db.Orders.AsNoTracking().SingleOrDefaultAsync(row => row.CustomerId == customerId && row.RequestId == requestId, cancellationToken);
        return order is null ? null : Receipt(order);
    }

    public async Task<OrderPlacementResult> PlaceAsync(Guid customerId, Guid requestId, BasketSnapshot basket, CheckoutInput contact, string quote, CancellationToken cancellationToken = default)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>();
                await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
                var existing = await db.Orders.AsNoTracking().SingleOrDefaultAsync(row => row.CustomerId == customerId && row.RequestId == requestId, cancellationToken);
                if (existing is not null) return new(Receipt(existing), null);
                if (!await db.Users.AnyAsync(user => user.Id == customerId, cancellationToken)) return new(null, "Your account is unavailable. Sign in again.");
                var products = await scope.ServiceProvider.GetRequiredService<ICheckoutCatalogReader>()
                    .GetManyAsync(basket.Lines.Select(line => line.ItemId).ToArray(), cancellationToken);
                var summary = CheckoutSummaryBuilder.Build(basket, products);
                if (!summary.CanReview) return new(null, summary.Errors.FirstOrDefault() ?? "Review your basket.");
                if (!string.Equals(CheckoutQuote.Fingerprint(summary), quote, StringComparison.Ordinal))
                    return new(null, "Prices or basket items changed. Review checkout again before placing the order.");
                var now = DateTimeOffset.UtcNow;
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customerId,
                    RequestId = requestId,
                    RestaurantId = products[0].RestaurantId,
                    OrderNumber = $"LE-{Guid.NewGuid():N}".ToUpperInvariant(),
                    Status = OrderStatus.Pending,
                    TotalMinor = summary.SubtotalMinor,
                    Currency = "BDT",
                    ContactName = contact.Name,
                    ContactPhone = contact.Phone,
                    DeliveryAddress = contact.Address,
                    Notes = contact.Notes,
                    PaymentMethod = contact.PaymentMethod,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                db.Orders.Add(order);
                await db.SaveChangesAsync(cancellationToken);
                db.OrderItems.AddRange(summary.Lines.Select(line => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    MenuItemId = line.ItemId,
                    ItemName = line.Name,
                    UnitPriceMinor = line.UnitPriceMinor,
                    Quantity = line.Quantity,
                    LineTotalMinor = line.TotalMinor
                }));
                await db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return new(Receipt(order), null);
            }
            catch (Exception exception) when (attempt < 2 && IsRetryable(exception))
            {
                // Disposal rolls back; retry with a fresh scope after serialization or uniqueness races.
            }
        }
        return new(null, "We couldn’t create the order. Please retry checkout.");
    }

    private static bool IsRetryable(Exception exception)
    {
        // Npgsql's execution strategy can wrap DbUpdateException in InvalidOperationException.
        for (Exception? current = exception; current is not null; current = current.InnerException)
            if (current is PostgresException { SqlState: "40001" or "23505" or "40P01" }) return true;
        return false;
    }
}
