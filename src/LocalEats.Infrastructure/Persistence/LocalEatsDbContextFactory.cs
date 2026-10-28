using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace LocalEats.Infrastructure.Persistence;

public sealed class LocalEatsDbContextFactory : IDesignTimeDbContextFactory<LocalEatsDbContext>
{
    public LocalEatsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Set ConnectionStrings__DefaultConnection explicitly to the LocalEats database at 127.0.0.1:15432 before using EF tools.");
        }
        var connection = new NpgsqlConnectionStringBuilder(connectionString);
        if (connection.Host != "127.0.0.1" || connection.Port != 15432 || connection.Database != "localeats")
        {
            throw new InvalidOperationException("Local EF tools allow only the localeats database at 127.0.0.1:15432. Other databases require a separately approved deployment workflow.");
        }

        var options = new DbContextOptionsBuilder<LocalEatsDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new LocalEatsDbContext(options);
    }
}
