using Xunit;

namespace LocalEats.Domain.Tests;

public sealed class ProjectSmokeTests
{
    [Fact]
    public void DomainAssembly_References_NoInfrastructureOrWebFramework()
    {
        var references = typeof(LocalEats.Domain.Entities.Restaurant).Assembly.GetReferencedAssemblies();

        Assert.DoesNotContain(references, reference =>
            reference.Name is not null &&
            (reference.Name.StartsWith("LocalEats.Infrastructure", StringComparison.Ordinal) ||
             reference.Name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) ||
             reference.Name.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal)));
    }
}
