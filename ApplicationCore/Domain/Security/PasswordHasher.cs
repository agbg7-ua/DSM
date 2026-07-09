using System.Security.Cryptography;

namespace ApplicationCore.Domain.Security;

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public static string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public static bool Verify(string password, string hashed)
    {
        var partes = hashed.Split('.', 3);
        if (partes.Length != 3 || !int.TryParse(partes[0], out int iterations))
        {
            return false;
        }

        byte[] salt;
        byte[] key;
        try
        {
            salt = Convert.FromBase64String(partes[1]);
            key = Convert.FromBase64String(partes[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        byte[] intento = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, key.Length);
        return CryptographicOperations.FixedTimeEquals(intento, key);
    }
}
