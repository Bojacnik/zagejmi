using System;

using Microsoft.Extensions.Options;

using Zagejmi.Application.Mail.Abstractions;

namespace Zagejmi.Infrastructure.Mail;

/// <summary>
///     Represents a factory for creating email verification links based on the configured public base URL.
/// </summary>
public sealed class EmailVerificationLinkFactory : IEmailVerificationLinkFactory
{
    private readonly EmailVerificationOptions options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EmailVerificationLinkFactory" /> class with the specified email
    ///     verification options.
    /// </summary>
    /// <param name="options">Options for email verification, including the public base URL.</param>
    public EmailVerificationLinkFactory(IOptions<EmailVerificationOptions> options)
    {
        this.options = options.Value;
    }

    /// <summary>
    ///     Creates an email verification link for the specified user ID and token. The link is constructed using the
    ///     configured public base URL.
    /// </summary>
    /// <param name="userId">The ID of the user for whom to create a verification link.</param>
    /// <param name="token">The verification token.</param>
    /// <returns>The email verification link.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the public base URL is not configured.</exception>
    public string Create(Guid userId, string token)
    {
        string baseUrl = this.options.PublicBaseUrl;
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("EmailVerification:PublicBaseUrl is not configured.");
        }

        string normalized = baseUrl.EndsWith('/') ? baseUrl[..^1] : baseUrl;
        return
            $"{normalized}/verify-email?userId={Uri.EscapeDataString(userId.ToString())}&token={Uri.EscapeDataString(token)}";
    }
}
