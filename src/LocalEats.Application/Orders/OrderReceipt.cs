namespace LocalEats.Application.Orders;

public sealed record OrderReceipt(Guid Id, string Number, long TotalMinor, string PaymentMethod, string Status);
