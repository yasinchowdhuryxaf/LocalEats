using LocalEats.Application.Basket;
using LocalEats.Application.Restaurants;
using LocalEats.Application.Security;
using LocalEats.Application.Users;
using LocalEats.Application.Users.Profile;
using LocalEats.Infrastructure.Identity;
using LocalEats.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LocalEats.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IRestaurantDirectoryReader, RestaurantDirectoryReader>();
        services.AddScoped<IRestaurantMenuReader, RestaurantMenuReader>();
        services.AddScoped<IBasketProductReader, BasketProductReader>();
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");

        services.AddDbContext<LocalEatsDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(LocalEatsDbContext).Assembly.FullName)));

        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 12;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
            .AddEntityFrameworkStores<LocalEatsDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "LocalEats.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
            options.LoginPath = "/login";
            options.AccessDeniedPath = "/access-denied";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
        });
        services.AddAuthorization(options =>
        {
            options.AddPolicy(RolePolicies.Customer, policy => policy.RequireAuthenticatedUser().RequireRole(RoleNames.Customer));
            options.AddPolicy(RolePolicies.Manager, policy => policy.RequireAuthenticatedUser().RequireRole(RoleNames.Manager));
            options.AddPolicy(RolePolicies.Admin, policy => policy.RequireAuthenticatedUser().RequireRole(RoleNames.Admin));
        });
        services.AddScoped<IOrderOwnershipReader, OrderOwnershipReader>();
        services.AddScoped<IResourceAuthorizationService, ResourceAuthorizationService>();
        services.AddScoped<IRegistrationStore, IdentityRegistrationStore>();
        services.AddScoped<ISessionStore, IdentitySessionStore>();
        services.AddScoped<IProfileStore, IdentityProfileStore>();

        services.AddHealthChecks()
            .AddCheck<Persistence.PostgresHealthCheck>("postgresql");

        return services;
    }
}
