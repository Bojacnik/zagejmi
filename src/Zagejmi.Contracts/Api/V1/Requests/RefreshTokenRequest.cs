namespace Zagejmi.Contracts.Api.V1.Requests;

/// <summary>
///     Request model for access-token renewal.
/// </summary>
public sealed class RefreshTokenRequest
{
    /// <summary>
    ///     Gets or sets the raw refresh token issued by the API.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}