using System;

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
    bool TryGetUserId(string jwtToken, out Guid userId);

    /// <summary>
    ///     Tries to read JWT ID claim from a JWT string.
    /// </summary>
    bool TryGetJwtId(string jwtToken, out string jwtId);
}