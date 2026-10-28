using LocalEats.Application.Basket;
using LocalEats.Application.Checkout;

namespace LocalEats.Application.Orders;

public interface IOrderPlacementStore
{
    Task<OrderReceipt?> FindAsync(Guid customerId, Guid requestId, CancellationToken cancellationToken = default);
    Task<OrderPlacementResult> PlaceAsync(Guid customerId, Guid requestId, BasketSnapshot basket, CheckoutInput contact, string quote, CancellationToken cancellationToken = default);
}
