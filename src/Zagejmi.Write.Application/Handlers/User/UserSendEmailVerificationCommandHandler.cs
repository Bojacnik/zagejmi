using System;
using System.Threading;
using System.Threading.Tasks;

using LanguageExt;

using Microsoft.Extensions.Logging;

using Zagejmi.Contracts.Commands;
using Zagejmi.Shared.Failures;
using Zagejmi.Write.Application.Abstractions;

namespace Zagejmi.Write.Application.Handlers.User;

/// <summary>
///     Sends email verification links for user accounts.
/// </summary>
public sealed class UserSendEmailVerificationCommandHandler
    : IHandlerRequest<UserSendEmailVerificationCommand, Either<Failure, Unit>>
{
    private static readonly Action<ILogger, string, Exception?> UserNotFoundLogger =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            default,
            "[EmailVerification] User not found for identifier: {UserIdentifier}");

    private static readonly Action<ILogger, Guid, Exception> EmailSendFailedLogger =
        LoggerMessage.Define<Guid>(
            LogLevel.Warning,
            default,
            "[EmailVerification] Failed to send verification email for user {UserId}");

    private readonly IEmailSender emailSender;
    private readonly IEmailVerificationLinkFactory emailVerificationLinkFactory;
    private readonly IEmailVerificationTokenService emailVerificationTokenService;
    private readonly IEventStore eventStore;
    private readonly ILogger<UserSendEmailVerificationCommandHandler> logger;
    private readonly IUserRepository userRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserSendEmailVerificationCommandHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="eventStore">The event store used to persist verification requests.</param>
    /// <param name="emailVerificationTokenService">The token generator/hash service.</param>
    /// <param name="emailVerificationLinkFactory">The verification link factory.</param>
    /// <param name="emailSender">The email sender.</param>
    /// <param name="logger">The logger.</param>
    public UserSendEmailVerificationCommandHandler(
        IUserRepository userRepository,
        IEventStore eventStore,
        IEmailVerificationTokenService emailVerificationTokenService,
        IEmailVerificationLinkFactory emailVerificationLinkFactory,
        IEmailSender emailSender,
        ILogger<UserSendEmailVerificationCommandHandler> logger)
    {
        this.userRepository = userRepository;
        this.eventStore = eventStore;
        this.emailVerificationTokenService = emailVerificationTokenService;
        this.emailVerificationLinkFactory = emailVerificationLinkFactory;
        this.emailSender = emailSender;
        this.logger = logger;
    }

    /// <summary>
    ///     Handles a request to send email verification.
    /// </summary>
    /// <param name="request">The send verification command.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Success when handled; otherwise a failure reason.</returns>
    public async Task<Either<Failure, Unit>> Handle(
        UserSendEmailVerificationCommand request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserIdentifier))
            {
                return new FailureArgumentInvalidValue("Username or email is required.");
            }

            Domain.Auth.User? user = request.UserIdentifier.Contains('@', StringComparison.Ordinal)
                ? await this.userRepository.GetByEmailAsync(request.UserIdentifier, cancellationToken)
                : await this.userRepository.GetByUsernameAsync(request.UserIdentifier, cancellationToken);

            if (user == null)
            {
                UserNotFoundLogger(this.logger, request.UserIdentifier, null);
                return new FailureDatabaseEntityNotFound("User not found.");
            }

            if (user.AuthCredentials?.EmailVerifiedAtUtc.HasValue == true)
            {
                return Unit.Default;
            }

            string token = this.emailVerificationTokenService.GenerateToken();
            string tokenHash = this.emailVerificationTokenService.HashToken(token);
            DateTimeOffset expiresAtUtc = DateTimeOffset.UtcNow.AddHours(24);

            user.RequestEmailVerification(tokenHash, expiresAtUtc);

            await this.eventStore.AppendAsync(
                user.Id,
                user.Version,
                user.DomainEvents,
                cancellationToken);

            string verificationLink = this.emailVerificationLinkFactory.Create(user.Id, token);
            string subject = "Verify your email";
            string body =
                $"<p>Hello {user.AuthCredentials?.UserName},</p><p>Click the link to verify your email:</p><p><a href=\"{verificationLink}\">Verify email</a></p><p>This link expires in 24 hours.</p>";

            try
            {
                await this.emailSender.SendEmailAsync(user.AuthCredentials!.Email, subject, body, cancellationToken);
            }
            catch (Exception ex)
            {
                EmailSendFailedLogger(this.logger, user.Id, ex);
                return new FailureRegister("Verification email could not be sent.");
            }

            return Unit.Default;
        }
        catch (Exception ex)
        {
            EmailSendFailedLogger(this.logger, Guid.Empty, ex);
            return new FailureRegister("Verification flow failed unexpectedly.");
        }
    }
}