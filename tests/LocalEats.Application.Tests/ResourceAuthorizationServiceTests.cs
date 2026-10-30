using System.Security.Claims;
using LocalEats.Application.Security;

namespace LocalEats.Application.Tests;

public sealed class ResourceAuthorizationServiceTests
{
    private sealed class Assignments(bool allowed) : IManagerRestaurantReader
    {
        public int Calls { get; private set; }
        public Guid ManagerId { get; private set; }
        public Guid RestaurantId { get; private set; }
        public Task<bool> IsAssignedAsync(Guid managerId, Guid restaurantId, CancellationToken cancellationToken = default)
        { Calls++; ManagerId = managerId; RestaurantId = restaurantId; return Task.FromResult(allowed); }
    }

    [Theory]
    [InlineData(RoleNames.Manager, true, true)]
    [InlineData(RoleNames.Manager, false, false)]
    [InlineData(RoleNames.Customer, true, false)]
    [InlineData(RoleNames.Admin, true, false)]
    public async Task ManageRestaurant_RequiresManagerRoleAndVerifiedAssignment(string role, bool assigned, bool expected)
    {
        var id = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var assignments = new Assignments(assigned);
        var service = new ResourceAuthorizationService(new RecordingReader(null), assignments);
        Assert.Equal(expected, await service.CanManageRestaurantAsync(Principal(id.ToString(), role), restaurantId));
        if (role == RoleNames.Manager)
        { Assert.Equal(id, assignments.ManagerId); Assert.Equal(restaurantId, assignments.RestaurantId); }
        else Assert.Equal(0, assignments.Calls);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ReadOrder_ManagerUsesPersistedRestaurantAssignment(bool assigned)
    {
        var order = new OrderOwnership(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var assignments = new Assignments(assigned);
        var id = Guid.NewGuid();
        var service = new ResourceAuthorizationService(new RecordingReader(order), assignments);
        Assert.Equal(assigned, await service.CanReadOrderAsync(Principal(id.ToString(), RoleNames.Manager), order.Id));
        Assert.Equal(id, assignments.ManagerId);
        Assert.Equal(order.RestaurantId, assignments.RestaurantId);
    }

    [Theory]
    [InlineData("invalid", true)]
    [InlineData("00000000-0000-0000-0000-000000000000", true)]
    [InlineData("valid", false)]
    public async Task ManageRestaurant_UntrustedIdentity_CannotLookup(string id, bool authenticated)
    {
        var assignments = new Assignments(true);
        var service = new ResourceAuthorizationService(new RecordingReader(null), assignments);
        Assert.False(await service.CanManageRestaurantAsync(Principal(id == "valid" ? Guid.NewGuid().ToString() : id, RoleNames.Manager, authenticated), Guid.NewGuid()));
        Assert.Equal(0, assignments.Calls);
    }

    [Theory]
    [InlineData(RoleNames.Customer, true, true)]
    [InlineData(RoleNames.Customer, false, false)]
    [InlineData(RoleNames.Manager, true, false)]
    [InlineData(RoleNames.Manager, false, false)]
    [InlineData(RoleNames.Admin, true, true)]
    [InlineData(RoleNames.Admin, false, true)]
    [InlineData("Unknown", true, false)]
    public async Task CanReadOrderAsync_RoleAndOwnership_EnforcesAccess(string role, bool isOwner, bool expected)
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var reader = new RecordingReader(new OrderOwnership(orderId, isOwner ? userId : Guid.NewGuid(), Guid.NewGuid()));
        var user = Principal(userId.ToString(), role);

        Assert.Equal(expected, await new ResourceAuthorizationService(reader, new Assignments(false)).CanReadOrderAsync(user, orderId));
    }

    [Theory]
    [InlineData(false, "valid", RoleNames.Admin)]
    [InlineData(true, "not-a-guid", RoleNames.Admin)]
    [InlineData(true, "00000000-0000-0000-0000-000000000000", RoleNames.Customer)]
    [InlineData(true, "valid", "Unknown")]
    public async Task CanReadOrderAsync_UntrustedIdentity_DeniesWithoutDatabaseLookup(bool authenticated, string id, string role)
    {
        var reader = new RecordingReader(null);
        var user = Principal(id == "valid" ? Guid.NewGuid().ToString() : id, role, authenticated);

        Assert.False(await new ResourceAuthorizationService(reader, new Assignments(false)).CanReadOrderAsync(user, Guid.NewGuid()));
        Assert.Equal(0, reader.Calls);
    }

    [Theory]
    [InlineData(RoleNames.Customer)]
    [InlineData(RoleNames.Admin)]
    public async Task CanReadOrderAsync_MissingOrder_DeniesEvenForAdmin(string role)
    {
        var service = new ResourceAuthorizationService(new RecordingReader(null), new Assignments(false));
        Assert.False(await service.CanReadOrderAsync(Principal(Guid.NewGuid().ToString(), role), Guid.NewGuid()));
    }

    private static ClaimsPrincipal Principal(string id, string role, bool authenticated = true) => new(new ClaimsIdentity(
        [new Claim(ClaimTypes.NameIdentifier, id), new Claim(ClaimTypes.Role, role)],
        authenticated ? "test" : null));

    private sealed class RecordingReader(OrderOwnership? order) : IOrderOwnershipReader
    {
        public int Calls { get; private set; }
        public Task<OrderOwnership?> FindAsync(Guid orderId, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(order);
        }
    }
}
