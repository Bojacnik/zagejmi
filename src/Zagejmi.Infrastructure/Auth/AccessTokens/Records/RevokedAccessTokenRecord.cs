using System;

namespace Zagejmi.Infrastructure.Auth.AccessTokens.Records;

/// <summary>
///     Persisted blacklist entry for revoked access token JWT IDs.
/// </summary>
public sealed record RevokedAccessTokenRecord
{
    /// <summary>
    ///     Gets or sets unique identifier for the revoked access token record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets the identifier of the user the token was issued to (users live in a separate DbContext).
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    ///     Gets or sets the JWT ID of the revoked access token.
    /// </summary>
    public string JwtId { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the date and time when the access token was revoked.
    /// </summary>
    public DateTimeOffset RevokedAt { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the access token expires.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    ///     Gets or sets the reason for revoking the access token.
    /// </summary>
    public string? Reason { get; set; }
}
