namespace LocalEats.Application.Users;

public static class ReturnUrlPolicy
{
    public static string Normalize(string? returnUrl)
    {
        return !string.IsNullOrWhiteSpace(returnUrl)
            && returnUrl[0] == '/'
            && (returnUrl.Length == 1 || returnUrl[1] != '/')
            && !returnUrl.Contains('\\')
            && !returnUrl.Any(char.IsControl)
                ? returnUrl
                : "/session";
    }
}
