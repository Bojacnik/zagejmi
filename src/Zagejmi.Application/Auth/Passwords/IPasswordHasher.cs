namespace Zagejmi.Application.Auth.Passwords;

/// <summary>
///     Hashes and verifies user passwords.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    ///     Hashes a plain text password.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <returns>The encoded hash, including the salt.</returns>
    string Hash(string password);

    /// <summary>
    ///     Verifies a plain text password against an encoded hash.
    /// </summary>
    /// <param name="passwordHash">The encoded hash produced by <see cref="Hash" />.</param>
    /// <param name="password">The plain text password.</param>
    /// <returns><see langword="true" /> when the password matches; otherwise <see langword="false" />.</returns>
    bool Verify(string passwordHash, string password);
}
