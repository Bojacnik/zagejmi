using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Zagejmi.Application.Abstractions;

namespace Zagejmi.Infrastructure.Auth;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions options;

    public SmtpEmailSender(IOptions<SmtpOptions> options)
    {
        this.options = options.Value;
    }

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