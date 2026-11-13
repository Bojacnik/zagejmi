using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Zagejmi.Application.Mail.Abstractions;
using Zagejmi.Infrastructure.Mail;

namespace Zagejmi.Infrastructure.Extensions;

/// <summary>
///     Provides extension methods for configuring and adding email-related services to the dependency injection container.
/// </summary>
public static class EmailServiceCollectionExtensions
{
    /// <summary>
    ///     Adds email-related services to the dependency injection container, including email verification token service,
    ///     email verification link factory, and SMTP email sender. Configures SMTP and email verification options from the
    ///     provided configuration.
    /// </summary>
    /// <param name="services">The IServiceCollection to which the email-related services will be added.</param>
    /// <param name="configuration">The configuration from which to configure the email settings.</param>
    /// <returns>The IServiceCollection with the email-related services added.</returns>
    public static IServiceCollection AddZagejmiEmail(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEmailVerificationTokenService, EmailVerificationTokenService>();
        services.AddScoped<IEmailVerificationLinkFactory, EmailVerificationLinkFactory>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
        services.Configure<EmailVerificationOptions>(configuration.GetSection("EmailVerification"));

        return services;
    }
}