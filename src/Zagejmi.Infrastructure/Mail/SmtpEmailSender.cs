using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Zagejmi.Application.Mail.Abstractions;

namespace Zagejmi.Infrastructure.Mail;

/// <summary>
///     Represents an email sender that uses SMTP (Simple Mail Transfer Protocol) to send emails.
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
    ///     Sends an email asynchronously using the specified parameters.
    /// </summary>
    /// <param name="toEmail">The email address of the recipient.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="htmlBody">The HTML body of the email.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        using MailMessage message = new();
        message.From = new MailAddress(this.options.FromEmail, this.options.FromName);
        message.To.Add(toEmail);
        message.Subject = subject;
        message.Body = htmlBody;
        message.IsBodyHtml = true;

        using SmtpClient client = new(this.options.Host, this.options.Port);
        client.EnableSsl = this.options.EnableSsl;

        if (this.options.UseAuthentication)
        {
            client.Credentials = new NetworkCredential(this.options.Username, this.options.Password);
        }

        cancellationToken.ThrowIfCancellationRequested();
        await client.SendMailAsync(message, cancellationToken);
    }
}