namespace Zagejmi.Read.Api.Models.Requests;

/// <summary>
///     Request model for resending email verification.
/// </summary>
public sealed class SendVerificationEmailRequest
{
    /// <summary>
    ///     Gets or sets username or email to identify the account.
    /// </summary>
    public string UserIdentifier { get; set; } = string.Empty;
}