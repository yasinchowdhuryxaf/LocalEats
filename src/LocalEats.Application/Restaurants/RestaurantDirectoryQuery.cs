namespace LocalEats.Application.Restaurants;

public sealed record RestaurantDirectoryQuery(string? Search = null, string? Area = null, string? Cuisine = null, int Page = 1);
