using System;
using System.Security.Cryptography;
using System.Text;

using Konscious.Security.Cryptography;

using Zagejmi.Application.Auth.Passwords;

namespace Zagejmi.Infrastructure.Auth.Passwords;

/// <summary>
///     Argon2id password hasher. The stored format is Base64(salt[16] + hash[16]).
/// </summary>
public sealed class Argon2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 16;

    /// <inheritdoc />
    public string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Compute(password, salt);

        byte[] result = new byte[SaltSize + HashSize];
        salt.CopyTo(result, 0);
        hash.CopyTo(result, SaltSize);

        return Convert.ToBase64String(result);
    }

    /// <inheritdoc />
    public bool Verify(string passwordHash, string password)
    {
        byte[] stored;
        try
        {
            stored = Convert.FromBase64String(passwordHash);
        }
        catch (FormatException)
        {
            return false;
        }

        if (stored.Length != SaltSize + HashSize)
        {
            return false;
        }

        byte[] computed = Compute(password, stored.AsSpan(0, SaltSize).ToArray());
        return CryptographicOperations.FixedTimeEquals(computed, stored.AsSpan(SaltSize));
    }

    private static byte[] Compute(string password, byte[] salt)
    {
        using Argon2id argon = new(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            Iterations = 4,
            MemorySize = 1024 * 16,
            DegreeOfParallelism = 2,
        };

        return argon.GetBytes(HashSize);
    }
}
