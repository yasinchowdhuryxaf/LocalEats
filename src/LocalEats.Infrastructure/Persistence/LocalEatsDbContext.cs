using LocalEats.Domain.Entities;
using LocalEats.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LocalEats.Infrastructure.Persistence;

public sealed class LocalEatsDbContext(DbContextOptions<LocalEatsDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();

    public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();

    public DbSet<MenuItem> MenuItems => Set<MenuItem>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LocalEatsDbContext).Assembly);
    }
}
