namespace LocalEats.Application.Basket;

public sealed class BasketService(IBasketStateStore store, IBasketProductReader products)
{
    public BasketSnapshot Snapshot() => store.Current.Snapshot();
    public async Task<BasketChange> AddAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        if (itemId == Guid.Empty) return new(false, "Choose an available menu item.");
        var product = await products.GetAsync(itemId, cancellationToken);
        return product is null ? new(false, "This item is unavailable or the restaurant is closed.") : store.Current.Add(product);
    }
    public Task<BasketChange> IncreaseAsync(Guid itemId, CancellationToken cancellationToken = default)
        => Snapshot().Lines.Any(line => line.ItemId == itemId)
            ? AddAsync(itemId, cancellationToken) : Task.FromResult(new BasketChange(false, "This item is not in your basket."));
    public BasketChange Decrease(Guid itemId) => store.Current.Decrease(itemId);
    public BasketChange Remove(Guid itemId) => store.Current.Remove(itemId);
    public BasketChange Clear() => store.Current.Clear();
}
