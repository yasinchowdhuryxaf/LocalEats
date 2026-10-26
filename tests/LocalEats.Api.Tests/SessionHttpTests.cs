using System.Net;
using System.Text.RegularExpressions;
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
        }

        await factory.DisposeAsync();
    }
}
