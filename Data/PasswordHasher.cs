using System.Security.Cryptography;
namespace RepairLab.Data;

public static class PasswordHasher
{
    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return Convert.ToBase64String(salt.Concat(key).ToArray());
    }
    public static bool Verify(string password, string stored)
    {
        var all = Convert.FromBase64String(stored);
        var salt = all[..16];
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(key, all[16..]);
    }
}
