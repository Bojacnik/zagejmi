namespace Zagejmi.Infrastructure.Mail;

/// <summary>
///     Represents the configuration options for email verification, including the public base URL used in verification
///     links.
/// </summary>
public sealed class EmailVerificationOptions
{
    /// <summary>
    ///     Gets or sets the public base URL used in email verification links. This URL is used to construct the verification.
    /// </summary>
    public required string PublicBaseUrl { get; set; }
}