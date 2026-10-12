using System.Security.Cryptography;
using System.Text;

namespace UserManagementAPI.Modules.Auth.Helpers;

internal static class PasswordVault
{
    public static string Protect(string password, string keyMaterial)
    {
        var key = SHA256.HashData(Encoding.UTF8.GetBytes(keyMaterial));
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor();
        var plain = Encoding.UTF8.GetBytes(password);
        var cipher = encryptor.TransformFinalBlock(plain, 0, plain.Length);
        var payload = new byte[aes.IV.Length + cipher.Length];
        Buffer.BlockCopy(aes.IV, 0, payload, 0, aes.IV.Length);
        Buffer.BlockCopy(cipher, 0, payload, aes.IV.Length, cipher.Length);
        return Convert.ToBase64String(payload);
    }

    public static string? Open(string? payload, string keyMaterial)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        var bytes = Convert.FromBase64String(payload);
        var key = SHA256.HashData(Encoding.UTF8.GetBytes(keyMaterial));
        using var aes = Aes.Create();
        aes.Key = key;
        var ivLength = aes.BlockSize / 8;
        aes.IV = bytes[..ivLength];
        using var decryptor = aes.CreateDecryptor();
        var plain = decryptor.TransformFinalBlock(bytes, ivLength, bytes.Length - ivLength);
        return Encoding.UTF8.GetString(plain);
    }
}
