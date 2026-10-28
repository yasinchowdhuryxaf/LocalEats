namespace LocalEats.Application.Basket;

public sealed record BasketSnapshot(IReadOnlyList<BasketLine> Lines)
{
    public int ItemCount => Lines.Sum(line => line.Quantity);
    public long SubtotalMinor => Lines.Sum(line => line.TotalMinor);
}
