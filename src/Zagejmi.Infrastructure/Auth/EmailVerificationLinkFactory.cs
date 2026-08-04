using System;

using Microsoft.Extensions.Options;

using Zagejmi.Application.Abstractions;

namespace Zagejmi.Infrastructure.Auth;

public sealed class EmailVerificationLinkFactory : IEmailVerificationLinkFactory
{
    private readonly EmailVerificationOptions options;

    public EmailVerificationLinkFactory(IOptions<EmailVerificationOptions> options)
    {
        this.options = options.Value;
    }

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