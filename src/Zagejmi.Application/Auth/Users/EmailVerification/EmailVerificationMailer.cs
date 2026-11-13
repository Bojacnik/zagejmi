using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Zagejmi.Application.Mail.Abstractions;
using Zagejmi.Application.Wallets;
using Zagejmi.Domain.Auth;

namespace Zagejmi.Application.Auth.Users.EmailVerification;

/// <summary>
///     Issues, checks and emails email verification tokens. Only a hash of the token is stored on the user.
/// </summary>
public sealed partial class EmailVerificationMailer
{
    /// <summary>
    ///     How long a verification link stays valid.
    /// </summary>
    public static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(24);

    /// <summary>
    ///     The shortest time between two verification emails for the same user.
    /// </summary>
    public static readonly TimeSpan ResendCooldown = TimeSpan.FromMinutes(1);

    private readonly IEmailVerificationTokenService tokenService;
    private readonly IEmailVerificationLinkFactory linkFactory;
    private readonly IEmailSender emailSender;
    private readonly TimeProvider timeProvider;
    private readonly WalletOptions walletOptions;
    private readonly ILogger<EmailVerificationMailer> logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EmailVerificationMailer" /> class.
    /// </summary>
    /// <param name="tokenService">Generates and hashes tokens.</param>
    /// <param name="linkFactory">Builds the verification link.</param>
    /// <param name="emailSender">Sends the email.</param>
    /// <param name="timeProvider">The clock.</param>
    /// <param name="walletOptions">The wallet rules (for the sign-up bonus mentioned in the email).</param>
    /// <param name="logger">The logger.</param>
    public EmailVerificationMailer(
        IEmailVerificationTokenService tokenService,
        IEmailVerificationLinkFactory linkFactory,
        IEmailSender emailSender,
        TimeProvider timeProvider,
        IOptions<WalletOptions> walletOptions,
        ILogger<EmailVerificationMailer> logger)
    {
        this.tokenService = tokenService;
        this.linkFactory = linkFactory;
        this.emailSender = emailSender;
        this.timeProvider = timeProvider;
        this.walletOptions = walletOptions.Value;
        this.logger = logger;
    }

    /// <summary>
    ///     Creates a new token and stores its hash and expiry on the user (the caller saves the user). Any older link
    ///     stops working.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>The plain token to put into the link.</returns>
    public string IssueToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        string token = this.tokenService.GenerateToken();
        user.AuthCredentials.PendingEmailVerificationTokenHash = this.tokenService.HashToken(token);
        user.AuthCredentials.PendingEmailVerificationExpiresAt = this.timeProvider.GetUtcNow() + TokenLifetime;
        return token;
    }

    /// <summary>
    ///     Gets a value indicating whether a new email may be sent (the last one is older than the cooldown).
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns><see langword="true" /> when another email may be sent.</returns>
    public bool CanResend(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        DateTimeOffset? expiresAt = user.AuthCredentials.PendingEmailVerificationExpiresAt;
        return expiresAt is null || this.timeProvider.GetUtcNow() >= expiresAt.Value - TokenLifetime + ResendCooldown;
    }

    /// <summary>
    ///     Checks a token from a verification link against the stored hash and expiry.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="token">The plain token.</param>
    /// <returns><see langword="true" /> when the token matches and has not expired.</returns>
    public bool IsValid(User user, string token)
    {
        ArgumentNullException.ThrowIfNull(user);

        string? storedHash = user.AuthCredentials.PendingEmailVerificationTokenHash;
        DateTimeOffset? expiresAt = user.AuthCredentials.PendingEmailVerificationExpiresAt;

        if (storedHash is null || expiresAt is null || this.timeProvider.GetUtcNow() >= expiresAt.Value)
        {
            return false;
        }

        byte[] expected = Encoding.UTF8.GetBytes(storedHash);
        byte[] actual = Encoding.UTF8.GetBytes(this.tokenService.HashToken(token));
        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }

    /// <summary>
    ///     Emails the verification link. A delivery failure is logged, not thrown: the user can ask for a new link.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="token">The plain token from <see cref="IssueToken" />.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true" /> when the email was handed to the mail server.</returns>
    public async Task<bool> SendAsync(User user, string token, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        string link = this.linkFactory.Create(user.Id, token);
        string bonus = this.walletOptions.SignUpBonus > 0
            ? $"<p>Po ověření vám na zákaznický účet připíšeme <strong>bonus {this.walletOptions.SignUpBonus} Goinů</strong>.</p>"
            : string.Empty;

        string body =
            $"""
             <p>Dobrý den, {WebUtility.HtmlEncode(user.AuthCredentials.UserName)},</p>
             <p>děkujeme za registraci na Zagejmi. Svůj e-mail ověříte kliknutím na tento odkaz:</p>
             <p><a href="{WebUtility.HtmlEncode(link)}">Ověřit e-mail</a></p>
             {bonus}
             <p>Odkaz platí 24 hodin. Pokud jste se neregistrovali, tento e-mail ignorujte.</p>
             """;

        try
        {
            await this.emailSender.SendEmailAsync(
                user.AuthCredentials.Email,
                "Ověřte svůj e-mail na Zagejmi",
                body,
                cancellationToken);

            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            this.LogSendFailed(ex, user.Id);
            return false;
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Sending the verification email to user {UserId} failed.")]
    private partial void LogSendFailed(Exception exception, Guid userId);
}
