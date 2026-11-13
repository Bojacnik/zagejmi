using System.Threading;
using System.Threading.Tasks;

namespace Zagejmi.Application.Mail.Abstractions;

/// <summary>
///     Defines a contract for sending emails asynchronously. Implementations of this interface are responsible for
///     handling the actual email sending process, which may involve interacting with an SMTP server, an email service
///     provider, or other email delivery mechanisms. The SendEmailAsync method allows for sending emails with specified
///     recipient addresses, subjects, and HTML content, while also supporting cancellation through a CancellationToken.
///     This interface is essential for managing email communication within an application, enabling developers to send
///     notifications, alerts, and other email-based messages to users in a reliable and efficient manner.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    ///     Sends an email asynchronously to the specified recipient with the given subject and HTML body content. The method
    ///     supports cancellation through a CancellationToken, allowing the calling code to cancel the email sending operation
    ///     if needed. Implementations of this method should handle the necessary steps to send the email, including formatting
    ///     the message, connecting to an email server or service, and managing any potential errors that may occur during the
    ///     sending process. The method should ensure that the email is sent successfully and provide appropriate feedback or
    ///     exceptions in case of failure.
    /// </summary>
    /// <param name="toEmail">
    ///     Email address of the recipient to whom the email will be sent. This parameter should be a valid
    ///     email address format and may include multiple recipients if supported by the implementation.
    /// </param>
    /// <param name="subject">Subject of the email to be sent.</param>
    /// <param name="htmlBody">HTML content of the email to be sent.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
}