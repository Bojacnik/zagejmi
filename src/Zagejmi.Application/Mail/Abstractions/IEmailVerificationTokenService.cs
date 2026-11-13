namespace Zagejmi.Application.Mail.Abstractions;

/// <summary>
///     Defines a contract for generating and hashing email verification tokens. Implementations of this interface are
///     responsible for creating secure and unique tokens that can be used for email verification purposes. The generated
///     tokens should be securely hashed to prevent unauthorized access and ensure the integrity of the email verification
///     process.
/// </summary>
public interface IEmailVerificationTokenService
{
    /// <summary>
    ///     Generates a unique and secure email verification token. The generated token should be random and difficult to
    ///     guess, ensuring that it can be used for secure email verification processes. Implementations of this method should
    ///     consider using cryptographic random number generators to create the token, and it should be of sufficient length to
    ///     prevent brute-force attacks. The generated token can be used in email verification links sent to users, allowing
    ///     them to confirm their email addresses securely.
    /// </summary>
    /// <returns>A unique and secure email verification token as a string.</returns>
    string GenerateToken();

    /// <summary>
    ///     Hashes the provided email verification token using a secure hashing algorithm. The hashed token should be stored in
    ///     the database instead of the plaintext token to prevent unauthorized access and ensure the integrity of the email
    ///     verification process. Implementations of this method should use a strong hashing algorithm (e.g., SHA-256, bcrypt).
    /// </summary>
    /// <param name="token">The email verification token to hash.</param>
    /// <returns>The hashed email verification token as a string.</returns>
    string HashToken(string token);
}
