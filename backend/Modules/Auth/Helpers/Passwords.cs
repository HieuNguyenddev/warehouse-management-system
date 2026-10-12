using Microsoft.AspNetCore.Identity;
using UserManagementAPI.Modules.Users.Entity;

namespace UserManagementAPI.Modules.Auth.Helpers;

internal static class Passwords
{
    private static readonly PasswordHasher<User> Hasher = new();

    public static string Hash(User user, string password)
    {
        return Hasher.HashPassword(user, password);
    }

    public static bool Verify(User user, string password)
    {
        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            return false;
        }

        return Hasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed;
    }
}
