using LocalEats.Application.Security;

namespace LocalEats.Infrastructure.Identity;

public static class ApplicationRoles
{
    public const string Customer = RoleNames.Customer;
    public const string Manager = RoleNames.Manager;
    public const string Admin = RoleNames.Admin;
}
