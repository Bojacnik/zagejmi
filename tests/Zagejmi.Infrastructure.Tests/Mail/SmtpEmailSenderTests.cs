using MailKit.Security;

using Xunit;

using Zagejmi.Infrastructure.Mail;

namespace Zagejmi.Infrastructure.Tests.Mail;

public sealed class SmtpEmailSenderTests
{
    [Theory]
    [InlineData(SmtpSecurity.Auto, 587, true, SecureSocketOptions.StartTls)]
    [InlineData(SmtpSecurity.Auto, 465, true, SecureSocketOptions.SslOnConnect)]
    [InlineData(SmtpSecurity.Auto, 1025, false, SecureSocketOptions.None)]
    [InlineData(SmtpSecurity.Auto, 465, false, SecureSocketOptions.None)]
    [InlineData(SmtpSecurity.StartTls, 465, false, SecureSocketOptions.StartTls)]
    [InlineData(SmtpSecurity.SslOnConnect, 587, true, SecureSocketOptions.SslOnConnect)]
    [InlineData(SmtpSecurity.None, 587, true, SecureSocketOptions.None)]
    public void Security_is_picked_from_the_setting_or_from_port_and_EnableSsl(
        SmtpSecurity security,
        int port,
        bool enableSsl,
        SecureSocketOptions expected)
    {
        SmtpOptions options = new()
        {
            Host = "mx.example.test",
            Port = port,
            EnableSsl = enableSsl,
            Security = security,
            Username = string.Empty,
            Password = string.Empty,
            FromEmail = "noreply@example.test",
        };

        Assert.Equal(expected, SmtpEmailSender.ToSocketOptions(options));
    }
}
