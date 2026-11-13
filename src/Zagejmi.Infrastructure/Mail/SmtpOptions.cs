namespace Zagejmi.Infrastructure.Mail;

/// <summary>
///     Represents the configuration options for SMTP (Simple Mail Transfer Protocol) used for sending emails.
/// </summary>
public sealed class SmtpOptions
{
    /// <summary>
    ///     Gets or sets the SMTP server host address. This is the address of the email server that will be used to send
    ///     emails.
    /// </summary>
    public required string Host { get; set; }

    /// <summary>
    ///     Gets or sets the port number for the SMTP server. The default value is set to 587, which is commonly used for
    ///     secure email submission.
    /// </summary>
    public int Port { get; set; } = 587;

    /// <summary>
    ///     Gets or sets a value indicating whether SSL (Secure Sockets Layer) should be enabled for the SMTP connection.
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether authentication should be used for the SMTP connection.
    /// </summary>
    public bool UseAuthentication { get; set; } = true;

    /// <summary>
    ///     Gets or sets the username for the SMTP connection.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    ///     Gets or sets the password for the SMTP connection.
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    ///     Gets or sets the email address of the sender.
    /// </summary>
    public required string FromEmail { get; set; }

    /// <summary>
    ///     Gets or sets the name of the sender.
    /// </summary>
    public string FromName { get; set; } = "Zagejmi";
}
