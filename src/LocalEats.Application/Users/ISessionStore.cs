namespace LocalEats.Application.Users;

public interface ISessionStore
{
    Task<bool> SignInAsync(string email, string password, bool rememberMe);

    Task SignOutAsync();
}
