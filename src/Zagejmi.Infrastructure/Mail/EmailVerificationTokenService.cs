using System;
using System.Security.Cryptography;
using System.Text;

using Zagejmi.Application.Mail.Abstractions;

namespace Zagejmi.Infrastructure.Mail;

/// <summary>
///     Represents a service responsible for generating and hashing email verification tokens. The
///     EmailVerificationTokenService class provides methods to create secure, random tokens for email verification
///     purposes and to hash these tokens using the SHA-256 algorithm. This service is essential for implementing email
///     verification workflows, ensuring that tokens are unique, difficult to guess, and securely stored in hashed form.
///     The generated tokens can be used in email verification links sent to users, while the hashed tokens can be stored
///     in a database for later validation.
/// </summary>
public sealed class EmailVerificationTokenService : IEmailVerificationTokenService
{
    /// <summary>
    ///     Generates a secure, random token for email verification purposes. The GenerateToken method creates a 32-byte
    ///     random token using a cryptographically secure random number generator, encodes it in Base64 format, and replaces
    ///     certain characters to make it URL-safe. The resulting token is trimmed of any trailing '=' characters and can be
    ///     used in email verification links sent to users. This method ensures that the generated tokens are unique and
    ///     difficult to guess, enhancing the security of the email verification process.
    /// </summary>
    /// <returns>Generated token as a URL-safe Base64 string.</returns>
    public string GenerateToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    /// <summary>
    ///     Hashes the provided token using the SHA-256 algorithm and returns the resulting hash as a hexadecimal string.
    /// </summary>
    /// <param name="token">The token to hash.</param>
    /// <returns>The hashed token as a hexadecimal string.</returns>
    public string HashToken(string token)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(token);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}