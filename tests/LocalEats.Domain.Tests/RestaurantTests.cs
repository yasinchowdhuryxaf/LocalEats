using LocalEats.Domain.Entities;

namespace LocalEats.Domain.Tests;

public sealed class RestaurantTests
{
    [Theory]
    [InlineData(false, false, false, false, false, false)]
    [InlineData(false, true, true, false, false, false)]
    [InlineData(true, false, true, false, false, false)]
    [InlineData(true, true, false, false, true, false)]
    [InlineData(true, true, true, false, true, true)]
    [InlineData(true, true, true, true, false, false)]
    public void Availability_StatusFlags_EnforceVisibilityAndOrdering(bool approved, bool active, bool open, bool deleted, bool visible, bool canOrder)
    {
        var restaurant = new Restaurant
        {
            IsApproved = approved,
            IsActive = active,
            IsOpen = open,
            DeletedAt = deleted ? DateTimeOffset.UtcNow : null
        };
        Assert.Equal(visible, restaurant.IsVisible);
        Assert.Equal(canOrder, restaurant.CanAcceptOrders);
    }

    [Fact]
    public void NewRestaurant_Defaults_CannotBeListedOrOrdered()
    {
        var restaurant = new Restaurant();
        Assert.False(restaurant.IsApproved);
        Assert.False(restaurant.IsActive);
        Assert.False(restaurant.IsOpen);
        Assert.False(restaurant.IsVisible);
        Assert.False(restaurant.CanAcceptOrders);
    }
}
