using Zagejmi.Write.Domain.Auth;

namespace Zagejmi.Write.Application.Abstractions;

/// <summary>
///     Service for generating authentication tokens for user profiles.
/// </summary>
public interface ITokenService
{
    /// <summary>
    ///     Generates a JWT token for the given user profile.
    /// </summary>
    /// <param name="user"> The user profile for which to generate the token.</param>
    /// <returns>Generated token for the user.</returns>
    string GenerateToken(User user);
}