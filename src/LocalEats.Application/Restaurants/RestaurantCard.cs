namespace LocalEats.Application.Restaurants;

public sealed record RestaurantCard(Guid Id, string Name, string? Description, string? Area, string? Cuisine, bool IsOpen, string Slug);
