using System;
using System.Text;

namespace Zagejmi.Api.Services;

/// <summary>
///     Verifies passwords against Argon2-based hashes stored in read models.
/// </summary>
public class PasswordHasherService
{
    /// <summary>
    ///     Verifies a plain text password against the stored hash for a user.
    /// </summary>
    /// <param name="user">User read model containing hash data.</param>
    /// <param name="password">Plain text password.</param>
    /// <returns><see langword="true" /> when password matches; otherwise <see langword="false" />.</returns>
    public bool VerifyPassword(UserReadModel user, string password)
    {
        try
        {
            byte[] hashBytes = Convert.FromBase64String(user.PasswordHash);
            Argon2id argon = new(Encoding.UTF8.GetBytes(password))
            {
                Salt = hashBytes.AsSpan(0, 16).ToArray(),
                Iterations = 4,
                MemorySize = 1024 * 16,
                DegreeOfParallelism = 2
            };

            byte[] computedHash = argon.GetBytes(16);
            return computedHash.AsSpan().SequenceEqual(hashBytes.AsSpan(16));
        }
        catch
        {
            return false;
        }
    }
}