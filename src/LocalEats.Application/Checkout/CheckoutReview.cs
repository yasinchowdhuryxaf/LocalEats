namespace LocalEats.Application.Checkout;

public sealed record CheckoutReview(CheckoutSummary Summary, IReadOnlyList<string> Errors);
