using System;

using Zagejmi.Domain.Auth;

namespace Zagejmi.Application.Auth.Tokens;

/// <summary>
///     Service for generating authentication tokens for user profiles.
/// </summary>
public interface ITokenService
{
    /// <summary>
    ///     Gets how long a newly issued refresh token stays valid.
    /// </summary>
    TimeSpan RefreshTokenLifetime { get; }

    /// <summary>
    ///     Generates a JWT token for the given user profile.
    /// </summary>
    /// <param name="user"> The user profile for which to generate the token.</param>
    /// <returns>Generated token for the user.</returns>
    string GenerateToken(User user);

    /// <summary>
    ///     Generates a cryptographically secure refresh token value.
    /// </summary>
    /// <returns>Opaque refresh token value.</returns>
    string GenerateRefreshToken();

    /// <summary>
    ///     Computes a deterministic hash for refresh-token storage and lookup.
    /// </summary>
    /// <param name="refreshToken">Raw refresh token.</param>
    /// <returns>Hex-encoded hash string.</returns>
    string HashRefreshToken(string refreshToken);

    /// <summary>
    ///     Tries to read the subject user ID from a JWT string.
    /// </summary>
    /// <returns>True if exists.</returns>
    bool TryGetUserId(string jwtToken, out Guid userId);
}