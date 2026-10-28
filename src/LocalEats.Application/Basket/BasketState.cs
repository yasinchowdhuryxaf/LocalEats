namespace LocalEats.Application.Basket;

public sealed class BasketState
{
    public const int MaxQuantity = 20;
    public const int MaxLines = 50;
    private readonly object gate = new();
    private readonly Dictionary<Guid, BasketLine> lines = [];

    public BasketSnapshot Snapshot() { lock (gate) return new(lines.Values.ToArray()); }

    public BasketChange Add(BasketProduct product)
    {
        lock (gate)
        {
            if (!product.CanOrder || product.Id == Guid.Empty || product.RestaurantId == Guid.Empty
                || product.PriceMinor < 0 || product.PriceMinor > long.MaxValue / (MaxLines * MaxQuantity))
                return new(false, "This item is unavailable or the restaurant is closed.");
            if (lines.Count != 0 && lines.Values.First().RestaurantId != product.RestaurantId)
                return new(false, "Your basket belongs to another restaurant. Clear it before adding from this restaurant.");
            var quantity = lines.TryGetValue(product.Id, out var existing) ? existing.Quantity + 1 : 1;
            if (quantity > MaxQuantity) return new(false, "You can add up to 20 of each item.");
            if (existing is null && lines.Count >= MaxLines) return new(false, "Your basket can contain up to 50 different items.");
            lines[product.Id] = new(product.Id, product.RestaurantId, product.RestaurantName, product.Name, product.PriceMinor, quantity);
            return new(true, "Basket updated.");
        }
    }

    public BasketChange Decrease(Guid itemId)
    {
        lock (gate)
        {
            if (!lines.TryGetValue(itemId, out var line)) return new(false, "This item is not in your basket.");
            if (line.Quantity == 1) lines.Remove(itemId);
            else lines[itemId] = line with { Quantity = line.Quantity - 1 };
            return new(true, "Basket updated.");
        }
    }

    public BasketChange Remove(Guid itemId)
    {
        lock (gate) return lines.Remove(itemId) ? new(true, "Item removed.") : new(false, "This item is not in your basket.");
    }

    public BasketChange Clear() { lock (gate) { lines.Clear(); return new(true, "Basket cleared."); } }
}
