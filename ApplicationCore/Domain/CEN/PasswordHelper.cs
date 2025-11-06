using System;
using System.Security.Cryptography;
using System.Text;

namespace ApplicationCore.Domain.Utils
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            using var hmac = new HMACSHA256();
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 2) return false;

            var salt = Convert.FromBase64String(parts[0]);
            var expectedHash = Convert.FromBase64String(parts[1]);

            using var hmac = new HMACSHA256(salt);
            var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            return CryptographicOperations.FixedTimeEquals(computed, expectedHash);
        }
    }
}
