using System.Security.Cryptography;

namespace CalendarTasking.Api.Security;

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public static bool Verify(string password, string hashedPassword)
    {
        var parts = hashedPassword.Split('.', 3);
        if (parts.Length != 3)
        {
            return false;
        }

        if (!int.TryParse(parts[0], out var iterations))
        {
            return false;
        }

        byte[] salt;
        byte[] expectedHash;

        try
        {
            salt = Convert.FromBase64String(parts[1]);
            expectedHash = Convert.FromBase64String(parts[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
