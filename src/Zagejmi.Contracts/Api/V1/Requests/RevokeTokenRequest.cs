namespace Zagejmi.Contracts.Api.V1.Requests;

/// <summary>
///     Request model for token/session revocation.
/// </summary>
public sealed class RevokeTokenRequest
{
    /// <summary>
    ///     Gets or sets the refresh token to revoke. If omitted and RevokeAllSessions is true,
    ///     all active refresh tokens for the user are revoked.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether all sessions for the current user should be revoked.
    /// </summary>
    public bool RevokeAllSessions { get; set; }
}