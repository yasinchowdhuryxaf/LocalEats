using Microsoft.AspNetCore.Identity;

namespace LocalEats.Infrastructure.Identity;

public static class DevelopmentRoleSeeder
{
    public static async Task SeedAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        foreach (var roleName in new[] { ApplicationRoles.Customer, ApplicationRoles.Manager, ApplicationRoles.Admin })
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Development role setup failed for {roleName}.");
            }
        }
    }
}
