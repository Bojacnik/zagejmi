using System;
using System.Threading;
using System.Threading.Tasks;

using MailKit.Net.Smtp;
using MailKit.Security;

using Microsoft.Extensions.Options;

using MimeKit;

using Zagejmi.Application.Mail.Abstractions;

namespace Zagejmi.Infrastructure.Mail;

/// <summary>
///     Represents an email sender that uses SMTP (Simple Mail Transfer Protocol) to send emails. Works with any
///     SMTP server: a relay on port 587 (STARTTLS), a mail server on port 465 (implicit TLS) or a local test server.
/// </summary>
public sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SmtpEmailSender" /> class with the specified SMTP options.
    /// </summary>
    /// <param name="options">
    ///     Options for configuring the SMTP email sender, including server details and authentication
    ///     settings.
    /// </param>
    public SmtpEmailSender(IOptions<SmtpOptions> options)
    {
        this.options = options.Value;
    }

    /// <summary>
    ///     Sends an HTML email message.
    /// </summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="htmlBody">The HTML body of the email.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the server has accepted the message.</returns>
    public async Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        using MimeMessage message = new();
        message.From.Add(new MailboxAddress(this.options.FromName, this.options.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using SmtpClient client = new();
        client.Timeout = (int)this.options.Timeout.TotalMilliseconds;

        await client.ConnectAsync(this.options.Host, this.options.Port, ToSocketOptions(this.options), cancellationToken);

        if (this.options.UseAuthentication)
        {
            await client.AuthenticateAsync(this.options.Username, this.options.Password, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken);
    }

    /// <summary>
    ///     Maps <see cref="SmtpOptions.Security" /> (and, for <see cref="SmtpSecurity.Auto" />, the port and
    ///     <see cref="SmtpOptions.EnableSsl" />) to how MailKit secures the connection.
    /// </summary>
    /// <param name="options">The SMTP options.</param>
    /// <returns>The MailKit socket options.</returns>
    internal static SecureSocketOptions ToSocketOptions(SmtpOptions options)
    {
        return options.Security switch
        {
            SmtpSecurity.StartTls => SecureSocketOptions.StartTls,
            SmtpSecurity.SslOnConnect => SecureSocketOptions.SslOnConnect,
            SmtpSecurity.None => SecureSocketOptions.None,
            _ when !options.EnableSsl => SecureSocketOptions.None,
            _ when options.Port == 465 => SecureSocketOptions.SslOnConnect,
            _ => SecureSocketOptions.StartTls,
        };
    }
}
