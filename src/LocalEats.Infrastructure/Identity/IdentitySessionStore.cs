using LocalEats.Application.Users;
using Microsoft.AspNetCore.Identity;

namespace LocalEats.Infrastructure.Identity;

public sealed class IdentitySessionStore(SignInManager<ApplicationUser> signIn) : ISessionStore
{
    public async Task<bool> SignInAsync(string email, string password, bool rememberMe)
    {
        var result = await signIn.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: true);
        return result.Succeeded;
    }

    public Task SignOutAsync() => signIn.SignOutAsync();
}
