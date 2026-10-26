using System.ComponentModel.DataAnnotations;

namespace LocalEats.Application.Users;

public sealed class LoginService(ISessionStore sessions)
{
    public async Task<bool> SignInAsync(LoginRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!Validator.TryValidateObject(request, new ValidationContext(request), [], true))
        {
            return false;
        }

        return await sessions.SignInAsync(request.Email.Trim(), request.Password, request.RememberMe);
    }

    public Task SignOutAsync() => sessions.SignOutAsync();
}
