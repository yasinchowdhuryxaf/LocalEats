using System.Security.Claims;
using LocalEats.Application.Users.Profile;

namespace LocalEats.Application.Tests;

public sealed class ProfileServiceTests
{
    [Fact]
    public async Task UpdateAsync_ValidInput_TrimsFieldsAndUsesIdentityId()
    {
        var id = Guid.NewGuid();
        var store = new RecordingStore();
        var result = await new ProfileService(store).UpdateAsync(Principal(id.ToString()), new UpdateProfileRequest
        {
            DisplayName = "  Test Name  ",
            PhoneNumber = " +8801712345678 ",
            Version = "version"
        });

        Assert.True(result.Succeeded);
        Assert.Equal(id, store.UserId);
        Assert.Equal("Test Name", store.Input?.DisplayName);
        Assert.Equal("+8801712345678", store.Input?.PhoneNumber);
    }

    [Fact]
    public async Task UpdateAsync_OptionalFieldsBlank_NormalizesPhoneToNull()
    {
        var store = new RecordingStore();
        var result = await new ProfileService(store).UpdateAsync(Principal(Guid.NewGuid().ToString()), new UpdateProfileRequest
        {
            DisplayName = " ",
            PhoneNumber = " ",
            Version = "version"
        });
        Assert.True(result.Succeeded);
        Assert.Null(store.Input?.PhoneNumber);
        Assert.Equal(string.Empty, store.Input?.DisplayName);
    }

    [Theory]
    [InlineData("name-too-long")]
    [InlineData("bad-phone")]
    [InlineData("phone-too-long")]
    [InlineData("missing-version")]
    public async Task UpdateAsync_InvalidFields_DoesNotWrite(string scenario)
    {
        var store = new RecordingStore();
        var request = new UpdateProfileRequest { DisplayName = "Name", Version = "version" };
        if (scenario == "name-too-long") request.DisplayName = new string('x', 101);
        if (scenario == "bad-phone") request.PhoneNumber = "not-a-phone";
        if (scenario == "phone-too-long") request.PhoneNumber = new string('1', 33);
        if (scenario == "missing-version") request.Version = string.Empty;

        var result = await new ProfileService(store).UpdateAsync(Principal(Guid.NewGuid().ToString()), request);

        Assert.False(result.Succeeded);
        Assert.NotEmpty(result.Errors);
        Assert.Equal(0, store.Writes);
    }

    [Theory]
    [InlineData(false, "valid")]
    [InlineData(true, "invalid")]
    [InlineData(true, "00000000-0000-0000-0000-000000000000")]
    public async Task ProfileAccess_InvalidIdentity_DeniesReadsAndWrites(bool authenticated, string id)
    {
        var store = new RecordingStore();
        var service = new ProfileService(store);
        var principal = Principal(id == "valid" ? Guid.NewGuid().ToString() : id, authenticated);
        Assert.Null(await service.GetAsync(principal));
        Assert.False((await service.UpdateAsync(principal, new UpdateProfileRequest { Version = "version" })).Succeeded);
        Assert.Equal(0, store.Reads);
        Assert.Equal(0, store.Writes);
    }

    [Fact]
    public async Task GetAsync_AuthenticatedUser_LoadsOnlyIdentityId()
    {
        var id = Guid.NewGuid();
        var store = new RecordingStore();
        await new ProfileService(store).GetAsync(Principal(id.ToString()));
        Assert.Equal(id, store.UserId);
        Assert.Equal(1, store.Reads);
    }

    private static ClaimsPrincipal Principal(string id, bool authenticated = true) => new(new ClaimsIdentity(
        [new Claim(ClaimTypes.NameIdentifier, id)], authenticated ? "test" : null));

    private sealed class RecordingStore : IProfileStore
    {
        public Guid UserId { get; private set; }
        public UpdateProfileRequest? Input { get; private set; }
        public int Reads { get; private set; }
        public int Writes { get; private set; }

        public Task<ProfileSnapshot?> GetAsync(Guid userId)
        {
            UserId = userId;
            Reads++;
            return Task.FromResult<ProfileSnapshot?>(null);
        }

        public Task<ProfileUpdateResult> UpdateAsync(Guid userId, UpdateProfileRequest request)
        {
            UserId = userId;
            Input = request;
            Writes++;
            return Task.FromResult(ProfileUpdateResult.Success());
        }
    }
}
