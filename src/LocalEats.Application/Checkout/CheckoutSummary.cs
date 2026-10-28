namespace LocalEats.Application.Checkout;

public sealed record CheckoutSummary(string? RestaurantName, IReadOnlyList<CheckoutLine> Lines, IReadOnlyList<string> Errors)
{
    public long SubtotalMinor => Lines.Sum(line => line.TotalMinor);
    public bool CanReview => Errors.Count == 0 && Lines.Count > 0;
}
