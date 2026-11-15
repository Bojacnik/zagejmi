namespace Zagejmi.Infrastructure.Mail;

/// <summary>
///     How the connection to the SMTP server is secured (<see cref="SmtpOptions.Security" />).
/// </summary>
public enum SmtpSecurity
{
    /// <summary>
    ///     Decided by the port: implicit TLS on 465, otherwise STARTTLS when <see cref="SmtpOptions.EnableSsl" /> is set
    ///     and a plain connection when it is not (the behaviour before this setting existed).
    /// </summary>
    Auto,

    /// <summary>
    ///     Plain connection, upgraded with STARTTLS; fails if the server does not offer it. Usually port 587.
    /// </summary>
    StartTls,

    /// <summary>
    ///     TLS from the first byte (SMTPS, "SSL/TLS"). Usually port 465.
    /// </summary>
    SslOnConnect,

    /// <summary>
    ///     No encryption at all. Only for local test servers such as Mailpit.
    /// </summary>
    None,
}
