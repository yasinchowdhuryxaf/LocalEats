namespace LocalEats.Application.Security;

public sealed record OrderOwnership(Guid Id, Guid? CustomerId);
