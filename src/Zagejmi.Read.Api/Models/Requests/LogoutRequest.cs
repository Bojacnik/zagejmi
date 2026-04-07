namespace Zagejmi.Read.Api.Models.Requests;

/// <summary>
///     Request model for user logout.
/// </summary>
public sealed class LogoutRequest
{
    /// <summary>
    ///     Gets or sets the URL to redirect to after logout.
    /// </summary>
    public string ReturnUrl { get; set; } = string.Empty;
}