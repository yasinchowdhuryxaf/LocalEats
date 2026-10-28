using System.Security.Claims;
using LocalEats.Application.Basket;
using LocalEats.Application.Checkout;

namespace LocalEats.Application.Tests;

public sealed class CheckoutServiceTests
{
    private static ClaimsPrincipal Customer() => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()), new Claim(ClaimTypes.Role, "Customer")], "test"));
    private sealed class Catalog(BasketProduct product) : ICheckoutCatalogReader
    {
        public Task<IReadOnlyList<BasketProduct>> GetManyAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<BasketProduct>>(ids.Contains(product.Id) ? [product] : []);
    }
    private static (CheckoutService Service, BasketSnapshot Basket) Fixture(bool available = true)
    {
        var item = new BasketProduct(Guid.NewGuid(), Guid.NewGuid(), "Current restaurant", "Current item", 25000, available);
        return (new CheckoutService(new Catalog(item)), new BasketSnapshot([new BasketLine(item.Id, Guid.NewGuid(), "Tampered", "Tampered", 1, 2)]));
    }

    [Fact]
    public async Task Summary_ValidBasket_UsesCurrentCatalogPriceAndNames()
    {
        var (service, basket) = Fixture();
        var summary = await service.SummaryAsync(Customer(), basket);
        Assert.True(summary.CanReview);
        Assert.Equal(50000, summary.SubtotalMinor);
        Assert.Equal("Current restaurant", summary.RestaurantName);
        Assert.Equal("Current item", Assert.Single(summary.Lines).Name);
    }

    [Theory]
    [InlineData("CashOnDelivery", "Sample address", true)]
    [InlineData("CashOnDelivery", "", false)]
    [InlineData("PayAtPickup", "", true)]
    [InlineData("Card", "Sample address", false)]
    public async Task Review_MethodAndAddress_ValidatesConditionalDelivery(string method, string address, bool valid)
    {
        var (service, basket) = Fixture();
        var result = await service.ReviewAsync(Customer(), basket, new CheckoutInput
        {
            Name = "Test Customer",
            Phone = "+8801712345678",
            PaymentMethod = method,
            Address = address
        });
        Assert.Equal(valid, result.Errors.Count == 0);
    }

    [Fact]
    public async Task Review_InvalidFields_RejectsWithoutPersisting()
    {
        var (service, basket) = Fixture();
        var result = await service.ReviewAsync(Customer(), basket, new CheckoutInput { Name = " ", Phone = "invalid", Notes = new string('x', 501) });
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task Summary_EmptyUnavailableOrAnonymous_BlocksReview()
    {
        var (service, basket) = Fixture(false);
        Assert.False((await service.SummaryAsync(Customer(), basket)).CanReview);
        Assert.False((await service.SummaryAsync(Customer(), new BasketSnapshot([]))).CanReview);
        Assert.False((await service.SummaryAsync(new ClaimsPrincipal(), basket)).CanReview);
    }
}
