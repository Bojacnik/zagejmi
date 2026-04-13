using System.Threading;
using System.Threading.Tasks;

namespace Zagejmi.Write.Application.Abstractions;

public interface IEmailSender
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
}