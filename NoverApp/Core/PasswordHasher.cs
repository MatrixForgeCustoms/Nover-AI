using System.Security.Cryptography;
using System.Text;

namespace NoverApp.Core;

// PBKDF2-SHA256 mit 200.000 Runden. Kompatibel mit den bisherigen Python-Konten.
public static class PasswordHasher
{
    public static string NewSalt() => Convert.ToHexString(RandomNumberGenerator.GetBytes(16));

    public static string Hash(string passwort, string salt) =>
        Convert.ToHexString(Rfc2898DeriveBytes.Pbkdf2(
            passwort, Encoding.UTF8.GetBytes(salt), 200_000, HashAlgorithmName.SHA256, 32));

    public static bool Verify(string passwort, string salt, string hash) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(Hash(passwort, salt)), Encoding.UTF8.GetBytes(hash));
}
