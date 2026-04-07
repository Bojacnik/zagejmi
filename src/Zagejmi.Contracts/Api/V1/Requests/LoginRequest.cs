namespace Zagejmi.Contracts.Api.V1.Requests;

/// <summary>
///     Request model for user login.
/// </summary>
public sealed class LoginRequest
{
    /// <summary>
    ///     Gets or sets the user identifier (username or email).
    /// </summary>
    public string UserIdentifier { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the password.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the URL to redirect to on successful login.
    /// </summary>
    public string SuccessUrl { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the URL to redirect to on failed login.
    /// </summary>
    public string FailureUrl { get; set; } = string.Empty;
}