using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using LocalEats.Application.Security;
using LocalEats.Domain.Entities;
using LocalEats.Infrastructure.Identity;
using LocalEats.Infrastructure.Persistence;
using LocalEats.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LocalEats.Api.Tests;

[Collection("PostgreSQL")]
public sealed class SessionHttpTests : IAsyncLifetime
{
    private WebApplicationFactory<WebEntryPoint>? factory;
    private readonly string email = $"session-{Guid.NewGuid():N}@example.test";
    private const string Password = "Session-Test-Password123!";
    private Guid? testRestaurantId;
    private Guid? extraUserId;

    public async Task InitializeAsync()
    {
        var connection = Environment.GetEnvironmentVariable("LOCALEATS_TEST_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=localeats_registration_tests;Username=localeats;Password=localeats_dev_password";
        var parsed = new Npgsql.NpgsqlConnectionStringBuilder(connection);
        if (parsed.Database?.EndsWith("_tests", StringComparison.Ordinal) != true)
        {
            throw new InvalidOperationException("HTTP session tests require a dedicated _tests database.");
        }

        factory = new WebApplicationFactory<WebEntryPoint>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:DefaultConnection", connection);
        });
        await using var scope = Factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>().Database.MigrateAsync();
        await DevelopmentRoleSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>());
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { Email = email, UserName = email };
        Assert.True((await users.CreateAsync(user, Password)).Succeeded);
        Assert.True((await users.AddToRoleAsync(user, ApplicationRoles.Customer)).Succeeded);
    }

    [Fact]
    public async Task ProtectedPage_Anonymous_RedirectsToLogin()
    {
        using var client = Client();
        var response = await client.GetAsync("/session");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/login?ReturnUrl=", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Login_ValidCredentials_SetsSecureCookieAndAllowsProtectedPage()
    {
        using var client = Client();
        var response = await Login(client, email, Password, "//example.com");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/session", response.Headers.Location?.ToString());
        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"), value => value.StartsWith("LocalEats.Auth=", StringComparison.Ordinal));
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/session")).StatusCode);
    }

    [Fact]
    public async Task Login_InvalidAndUnknownCredentials_ReturnSameSafeMessage()
    {
        using var client = Client();
        foreach (var address in new[] { email, "unknown@example.test" })
        {
            var response = await Login(client, address, "Wrong-Password123!");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Unable to sign in. Check your details or try again later.", await response.Content.ReadAsStringAsync());
            Assert.DoesNotContain(response.Headers.Where(header => header.Key == "Set-Cookie")
                .SelectMany(header => header.Value), value => value.StartsWith("LocalEats.Auth=", StringComparison.Ordinal));
        }
    }

    [Fact]
    public async Task Logout_ValidPost_ClearsCookieAndBlocksProtectedPage()
    {
        using var client = Client();
        Assert.Equal(HttpStatusCode.Redirect, (await Login(client, email, Password)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/logout")).StatusCode);
        // GET renders confirmation; it must not sign the user out.
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/session")).StatusCode);
        var fields = await FormFields(client, "/logout");
        var response = await client.PostAsync("/logout", new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/login", response.Headers.Location?.ToString());
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/session")).StatusCode);
    }

    [Fact]
    public async Task Login_PostWithoutAntiforgeryToken_IsRejected()
    {
        using var client = Client();
        var response = await client.PostAsync("/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["_handler"] = "login",
            ["Input.Email"] = email,
            ["Input.Password"] = Password
        }));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Logout_PostWithoutAntiforgeryToken_PreservesSession()
    {
        using var client = Client();
        Assert.Equal(HttpStatusCode.Redirect, (await Login(client, email, Password)).StatusCode);
        var response = await client.PostAsync("/logout", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["_handler"] = "logout"
        }));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/session")).StatusCode);
    }

    [Fact]
    public async Task Login_RememberMe_CreatesPersistentCookie()
    {
        using var client = Client();
        var response = await Login(client, email, Password, rememberMe: true);
        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"), value => value.StartsWith("LocalEats.Auth=", StringComparison.Ordinal));
        Assert.Contains("expires=", cookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_FiveFailures_LocksAccountAndKeepsResponseGeneric()
    {
        using var client = Client();
        for (var attempt = 0; attempt < 5; attempt++)
        {
            await Login(client, email, "Wrong-Password123!");
        }

        var response = await Login(client, email, Password);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Unable to sign in. Check your details or try again later.", await response.Content.ReadAsStringAsync());
        await using var scope = Factory.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByEmailAsync(email);
        Assert.NotNull(user);
        Assert.True(await users.IsLockedOutAsync(user));
    }

    [Theory]
    [InlineData(ApplicationRoles.Customer, "/session/customer", true)]
    [InlineData(ApplicationRoles.Customer, "/manager", false)]
    [InlineData(ApplicationRoles.Customer, "/admin", false)]
    [InlineData(ApplicationRoles.Manager, "/session/customer", false)]
    [InlineData(ApplicationRoles.Manager, "/manager", true)]
    [InlineData(ApplicationRoles.Manager, "/admin", false)]
    [InlineData(ApplicationRoles.Admin, "/session/customer", false)]
    [InlineData(ApplicationRoles.Admin, "/manager", false)]
    [InlineData(ApplicationRoles.Admin, "/admin", true)]
    public async Task RolePages_SignedInRole_EnforcesPolicyOnServer(string role, string path, bool allowed)
    {
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await users.FindByEmailAsync(email);
            Assert.NotNull(user);
            Assert.True((await users.RemoveFromRoleAsync(user, ApplicationRoles.Customer)).Succeeded);
            Assert.True((await users.AddToRoleAsync(user, role)).Succeeded);
        }

        using var client = Client();
        Assert.Equal(HttpStatusCode.Redirect, (await Login(client, email, Password)).StatusCode);
        var response = await client.GetAsync(path);
        if (allowed)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        else
        {
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/access-denied", response.Headers.Location?.ToString());
        }
    }

    [Theory]
    [InlineData("/session/customer")]
    [InlineData("/manager")]
    [InlineData("/admin")]
    public async Task RolePages_Anonymous_RedirectsToLogin(string path)
    {
        using var client = Client();
        var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/login?ReturnUrl=", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task OrderOwnership_LoadedFromPostgres_DeniesChangedIdAndAllowsExplicitAdmin()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByEmailAsync(email);
        Assert.NotNull(user);
        var database = scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>();
        var now = DateTimeOffset.UtcNow;
        var restaurant = new Restaurant
        {
            Id = Guid.NewGuid(),
            Name = "Authorization test",
            Slug = $"authorization-{Guid.NewGuid():N}",
            CreatedAt = now,
            UpdatedAt = now
        };
        testRestaurantId = restaurant.Id;
        var ownOrder = new Order
        {
            Id = Guid.NewGuid(),
            Restaurant = restaurant,
            CustomerId = user.Id,
            OrderNumber = $"OWN-{Guid.NewGuid():N}",
            CreatedAt = now,
            UpdatedAt = now
        };
        var otherOrder = new Order
        {
            Id = Guid.NewGuid(),
            Restaurant = restaurant,
            CustomerId = Guid.NewGuid(),
            OrderNumber = $"OTHER-{Guid.NewGuid():N}",
            CreatedAt = now,
            UpdatedAt = now
        };
        database.Orders.AddRange(ownOrder, otherOrder);
        await database.SaveChangesAsync();
        var authorization = scope.ServiceProvider.GetRequiredService<IResourceAuthorizationService>();
        var customer = await scope.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<ApplicationUser>>().CreateAsync(user);

        Assert.True(await authorization.CanReadOrderAsync(customer, ownOrder.Id));
        Assert.False(await authorization.CanReadOrderAsync(customer, otherOrder.Id));
        Assert.False(await authorization.CanReadOrderAsync(customer, Guid.NewGuid()));
        Assert.True((await users.AddToRoleAsync(user, ApplicationRoles.Admin)).Succeeded);
        var admin = await scope.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<ApplicationUser>>().CreateAsync(user);
        Assert.True(await authorization.CanReadOrderAsync(admin, otherOrder.Id));
        Assert.False(await authorization.CanReadOrderAsync(new ClaimsPrincipal(), ownOrder.Id));
    }

    [Fact]
    public async Task Account_Anonymous_RedirectsToLogin()
    {
        using var client = Client();
        var response = await client.GetAsync("/account");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/login?ReturnUrl=", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Account_SaveProfile_PersistsAndKeepsSessionWithoutVerifyingPhone()
    {
        using var client = Client();
        await Login(client, email, Password);
        var fields = await FormFields(client, "/account");
        fields["Input.DisplayName"] = "Test Account";
        fields["Input.PhoneNumber"] = "+8801712345678";
        fields["Input.PhoneNumberConfirmed"] = "true";
        var response = await client.PostAsync("/account", new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var page = await client.GetStringAsync(response.Headers.Location);
        Assert.Contains("Your profile was saved.", page);
        Assert.Contains("Test Account", page);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/session")).StatusCode);
        await using var scope = Factory.Services.CreateAsyncScope();
        var user = await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(email);
        Assert.NotNull(user);
        Assert.Equal("Test Account", user.DisplayName);
        Assert.Equal("+8801712345678", user.PhoneNumber);
        Assert.False(user.PhoneNumberConfirmed);
        Assert.Equal(email, user.UserName);
    }

    [Fact]
    public async Task Account_InvalidPhone_DoesNotPersistAnyProfileChanges()
    {
        using var client = Client();
        await Login(client, email, Password);
        var fields = await FormFields(client, "/account");
        fields["Input.DisplayName"] = "Should not save";
        fields["Input.PhoneNumber"] = "not-a-phone";
        var response = await client.PostAsync("/account", new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Enter a valid phone number", await response.Content.ReadAsStringAsync());
        await using var scope = Factory.Services.CreateAsyncScope();
        var user = await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(email);
        Assert.NotNull(user);
        Assert.Equal(string.Empty, user.DisplayName);
        Assert.Null(user.PhoneNumber);
    }

    [Fact]
    public async Task Account_ChangedUserIdAndEmail_UpdateOnlySignedInProfile()
    {
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var otherEmail = $"other-{Guid.NewGuid():N}@example.test";
            var other = new ApplicationUser { UserName = otherEmail, Email = otherEmail, DisplayName = "Other user" };
            Assert.True((await users.CreateAsync(other, Password)).Succeeded);
            extraUserId = other.Id;
        }

        using var client = Client();
        await Login(client, email, Password);
        var path = $"/account?userId={extraUserId}";
        var fields = await FormFields(client, path);
        fields["Input.UserId"] = extraUserId.ToString() ?? string.Empty;
        fields["Input.Email"] = "changed@example.test";
        fields["Input.DisplayName"] = "My name";
        fields["Input.PhoneNumber"] = string.Empty;
        var response = await client.PostAsync(path, new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        await using var verify = Factory.Services.CreateAsyncScope();
        var manager = verify.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var current = await manager.FindByEmailAsync(email);
        Assert.NotNull(current);
        Assert.Equal("My name", current.DisplayName);
        Assert.Equal(email, current.Email);
        var otherUser = await manager.FindByIdAsync(extraUserId.ToString() ?? string.Empty);
        Assert.NotNull(otherUser);
        Assert.Equal("Other user", otherUser.DisplayName);
    }

    [Fact]
    public async Task Account_StaleForm_DoesNotOverwriteNewerProfile()
    {
        using var client = Client();
        await Login(client, email, Password);
        var first = await FormFields(client, "/account");
        var stale = await FormFields(client, "/account");
        first["Input.DisplayName"] = "First edit";
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/account", new FormUrlEncodedContent(first))).StatusCode);
        stale["Input.DisplayName"] = "Stale edit";
        var response = await client.PostAsync("/account", new FormUrlEncodedContent(stale));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("changed in another session", await response.Content.ReadAsStringAsync());
        await using var scope = Factory.Services.CreateAsyncScope();
        var user = await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(email);
        Assert.NotNull(user);
        Assert.Equal("First edit", user.DisplayName);
    }

    [Fact]
    public async Task Account_PostWithoutAntiforgeryToken_IsRejected()
    {
        using var client = Client();
        await Login(client, email, Password);
        var response = await client.PostAsync("/account", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["_handler"] = "profile",
            ["Input.DisplayName"] = "Unprotected change"
        }));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Account_DeletedUser_ShowsSafeLoadError()
    {
        using var client = Client();
        await Login(client, email, Password);
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>().Users
                .Where(user => user.Email == email).ExecuteDeleteAsync();
        }

        var response = await client.GetAsync("/account");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("We couldn't load your profile", WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync()));
    }

    [Fact]
    public async Task Account_ChangedPreviouslyVerifiedPhone_RequiresVerificationAgain()
    {
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await users.FindByEmailAsync(email);
            Assert.NotNull(user);
            user.PhoneNumber = "+8801712345678";
            user.PhoneNumberConfirmed = true;
            Assert.True((await users.UpdateAsync(user)).Succeeded);
        }

        using var client = Client();
        await Login(client, email, Password);
        var fields = await FormFields(client, "/account");
        fields["Input.DisplayName"] = "Changed phone";
        fields["Input.PhoneNumber"] = "+8801812345678";
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/account", new FormUrlEncodedContent(fields))).StatusCode);
        await using var verify = Factory.Services.CreateAsyncScope();
        var current = await verify.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(email);
        Assert.NotNull(current);
        Assert.False(current.PhoneNumberConfirmed);
        Assert.Equal("+8801812345678", current.PhoneNumber);
    }

    [Fact]
    public async Task Account_HtmlInName_RendersAsText()
    {
        using var client = Client();
        await Login(client, email, Password);
        var fields = await FormFields(client, "/account");
        fields["Input.DisplayName"] = "<script>alert(1)</script>";
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/account", new FormUrlEncodedContent(fields))).StatusCode);
        var html = await client.GetStringAsync("/account");
        Assert.Contains("&lt;script&gt;", html);
        Assert.DoesNotContain("<script>alert(1)</script>", html);
    }

    private WebApplicationFactory<WebEntryPoint> Factory => factory ?? throw new InvalidOperationException("Not initialized.");

    private HttpClient Client() => Factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        BaseAddress = new Uri("https://localhost")
    });

    private static async Task<HttpResponseMessage> Login(HttpClient client, string address, string password, string returnUrl = "/session", bool rememberMe = false)
    {
        var path = "/login?ReturnUrl=" + Uri.EscapeDataString(returnUrl);
        var fields = await FormFields(client, path);
        fields["Input.Email"] = address;
        fields["Input.Password"] = password;
        fields["Input.RememberMe"] = rememberMe.ToString();
        return await client.PostAsync(path, new FormUrlEncodedContent(fields));
    }

    private static async Task<Dictionary<string, string>> FormFields(HttpClient client, string path)
    {
        var response = await client.GetAsync(path);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        var fields = new Dictionary<string, string>();
        foreach (Match match in Regex.Matches(html, "<input[^>]*type=\"hidden\"[^>]*>"))
        {
            var name = Regex.Match(match.Value, "name=\"([^\"]*)\"").Groups[1].Value;
            var value = Regex.Match(match.Value, "value=\"([^\"]*)\"").Groups[1].Value;
            fields[WebUtility.HtmlDecode(name)] = WebUtility.HtmlDecode(value);
        }

        Assert.Contains("__RequestVerificationToken", fields.Keys);
        Assert.Contains("_handler", fields.Keys);
        return fields;
    }

    public async Task DisposeAsync()
    {
        if (factory is null)
        {
            return;
        }

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>().Users
                .Where(user => user.Email == email).ExecuteDeleteAsync();
            if (extraUserId is Guid otherUserId)
            {
                await scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>().Users
                    .Where(user => user.Id == otherUserId).ExecuteDeleteAsync();
            }
            if (testRestaurantId is Guid restaurantId)
            {
                var database = scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>();
                await database.Orders.Where(order => order.RestaurantId == restaurantId).ExecuteDeleteAsync();
                await database.Restaurants.Where(restaurant => restaurant.Id == restaurantId).ExecuteDeleteAsync();
            }
        }

        await factory.DisposeAsync();
    }
}
