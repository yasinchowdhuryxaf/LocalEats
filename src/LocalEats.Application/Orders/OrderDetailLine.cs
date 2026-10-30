namespace LocalEats.Application.Orders;

public sealed record OrderDetailLine(string Name, int Quantity, long UnitPriceMinor, long TotalMinor);
