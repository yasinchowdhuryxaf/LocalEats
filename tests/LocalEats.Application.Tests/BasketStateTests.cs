using LocalEats.Application.Basket;

namespace LocalEats.Application.Tests;

public sealed class BasketStateTests
{
    private static BasketProduct Product(Guid? restaurant = null, bool available = true, long price = 12345)
        => new(Guid.NewGuid(), restaurant ?? Guid.NewGuid(), "Restaurant", "Item", price, available);

    [Fact]
    public void AddAndDecrease_ValidItem_UpdatesSubtotalAndClearsLastLine()
    {
        var state = new BasketState();
        var product = Product();
        Assert.True(state.Add(product).Succeeded);
        Assert.True(state.Add(product).Succeeded);
        Assert.Equal(2, state.Snapshot().ItemCount);
        Assert.Equal(24690, state.Snapshot().SubtotalMinor);
        state.Decrease(product.Id);
        Assert.Equal(12345, state.Snapshot().SubtotalMinor);
        state.Decrease(product.Id);
        Assert.Empty(state.Snapshot().Lines);
        Assert.Equal(0, state.Snapshot().SubtotalMinor);
        Assert.True(state.Add(Product()).Succeeded);
    }

    [Fact]
    public void Add_DifferentRestaurant_RejectsWithoutChangingBasket()
    {
        var state = new BasketState();
        state.Add(Product());
        Assert.False(state.Add(Product()).Succeeded);
        Assert.Single(state.Snapshot().Lines);
    }

    [Theory]
    [InlineData(false, 100)]
    [InlineData(true, -1)]
    [InlineData(true, long.MaxValue)]
    public void Add_InvalidProduct_LeavesEmptyBasket(bool available, long price)
    {
        var state = new BasketState();
        Assert.False(state.Add(Product(available: available, price: price)).Succeeded);
        Assert.Empty(state.Snapshot().Lines);
    }

    [Fact]
    public void Add_QuantityAndLineLimits_RejectsExcess()
    {
        var state = new BasketState();
        var product = Product();
        for (var i = 0; i < 20; i++) Assert.True(state.Add(product).Succeeded);
        Assert.False(state.Add(product).Succeeded);
        Assert.Equal(20, state.Snapshot().ItemCount);
        for (var i = 1; i < 50; i++) Assert.True(state.Add(Product(product.RestaurantId)).Succeeded);
        Assert.False(state.Add(Product(product.RestaurantId)).Succeeded);
        Assert.Equal(50, state.Snapshot().Lines.Count);
    }

    [Fact]
    public void RemoveAndClear_ExistingLines_ResetRestaurantAndTotals()
    {
        var state = new BasketState();
        var product = Product();
        state.Add(product);
        Assert.True(state.Remove(product.Id).Succeeded);
        Assert.True(state.Add(Product()).Succeeded);
        state.Clear();
        Assert.Empty(state.Snapshot().Lines);
        Assert.False(state.Remove(Guid.NewGuid()).Succeeded);
    }

    [Fact]
    public async Task Add_ConcurrentCalls_EnforcesQuantityLimitAtomically()
    {
        var state = new BasketState();
        var product = Product();
        await Task.WhenAll(Enumerable.Range(0, 100).Select(_ => Task.Run(() => state.Add(product))));
        Assert.Equal(20, state.Snapshot().ItemCount);
    }
}
