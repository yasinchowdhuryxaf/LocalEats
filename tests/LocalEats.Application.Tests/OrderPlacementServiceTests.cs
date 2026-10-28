using System.Security.Claims;
using LocalEats.Application.Basket;
using LocalEats.Application.Checkout;
using LocalEats.Application.Orders;

namespace LocalEats.Application.Tests;

public sealed class OrderPlacementServiceTests
{
    private sealed class Store : IOrderPlacementStore
    {
        public int Reads { get; private set; }
        public int Writes { get; private set; }
        public Task<OrderReceipt?> FindAsync(Guid customerId, Guid requestId, CancellationToken cancellationToken = default)
        { Reads++; return Task.FromResult<OrderReceipt?>(null); }
        public Task<OrderPlacementResult> PlaceAsync(Guid customerId, Guid requestId, BasketSnapshot basket, CheckoutInput contact, string quote, CancellationToken cancellationToken = default)
        { Writes++; return Task.FromResult(new OrderPlacementResult(null, "Unexpected persistence.")); }
    }

    [Theory]
    [InlineData("anonymous")]
    [InlineData("Manager")]
    [InlineData("Admin")]
    [InlineData("invalid-id")]
    public async Task Place_InvalidCustomer_CannotReadOrWrite(string scenario)
    {
        var store = new Store();
        var user = scenario == "anonymous" ? new ClaimsPrincipal() : new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.Role, scenario == "invalid-id" ? "Customer" : scenario),
            new Claim(ClaimTypes.NameIdentifier, scenario == "invalid-id" ? "invalid" : Guid.NewGuid().ToString())], "test"));
        var result = await new OrderPlacementService(store).PlaceAsync(user, Guid.NewGuid(), new BasketSnapshot([]), new CheckoutInput(), "quote");
        Assert.False(result.Succeeded);
        Assert.Null(await new OrderPlacementService(store).FindAsync(user, Guid.NewGuid()));
        Assert.Equal(0, store.Reads);
        Assert.Equal(0, store.Writes);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Place_InvalidContactOrMissingReview_CannotPersist(bool invalidContact)
    {
        var store = new Store();
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Customer"), new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())], "test"));
        var basket = new BasketSnapshot([new BasketLine(Guid.NewGuid(), Guid.NewGuid(), "Restaurant", "Item", 100, 1)]);
        var contact = new CheckoutInput { Name = invalidContact ? " " : "Customer", Phone = "+8801712345678", Address = "Sample road" };
        var result = await new OrderPlacementService(store).PlaceAsync(user, Guid.NewGuid(), basket, contact, invalidContact ? "quote" : null);
        Assert.False(result.Succeeded);
        Assert.Equal(0, store.Writes);
    }
}
