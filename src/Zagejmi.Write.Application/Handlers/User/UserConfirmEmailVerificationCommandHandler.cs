using System;
using System.Threading;
using System.Threading.Tasks;

using LanguageExt;

using Zagejmi.Contracts.Commands;
using Zagejmi.Shared.Failures;
using Zagejmi.Write.Application.Abstractions;

namespace Zagejmi.Write.Application.Handlers.User;

public sealed class UserConfirmEmailVerificationCommandHandler
    : IHandlerRequest<UserConfirmEmailVerificationCommand, Either<Failure, Unit>>
{
    private readonly IEmailVerificationTokenService emailVerificationTokenService;
    private readonly IEventStore eventStore;
    private readonly IUserRepository userRepository;

    public UserConfirmEmailVerificationCommandHandler(
        IUserRepository userRepository,
        IEventStore eventStore,
        IEmailVerificationTokenService emailVerificationTokenService)
    {
        this.userRepository = userRepository;
        this.eventStore = eventStore;
        this.emailVerificationTokenService = emailVerificationTokenService;
    }

    public async Task<Either<Failure, Unit>> Handle(
        UserConfirmEmailVerificationCommand request,
        CancellationToken cancellationToken = default)
    {
        if (request.UserId == Guid.Empty)
        {
            return new FailureArgumentInvalidValue("User id is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return new FailureArgumentInvalidValue("Verification token is required.");
        }

        Domain.Auth.User? user = await this.userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return new FailureDatabaseEntityNotFound("User not found.");
        }

        if (user.AuthCredentials?.EmailVerifiedAtUtc.HasValue == true)
        {
            return Unit.Default;
        }

        string providedHash = this.emailVerificationTokenService.HashToken(request.Token);
        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;

        if (user.AuthCredentials?.PendingEmailVerificationExpiresAtUtc.HasValue == true &&
            user.AuthCredentials.PendingEmailVerificationExpiresAtUtc.Value < nowUtc)
        {
            return new VerificationFailureEmailTokenExpired("Verification token has expired. Request a new email.");
        }

        if (!user.CanConfirmEmail(providedHash, nowUtc))
        {
            return new VerificationFailureEmailTokenInvalid("Verification token is invalid.");
        }

        user.ConfirmEmailOwnership();

        await this.eventStore.AppendAsync(
            user.Id,
            user.Version,
            user.DomainEvents,
            cancellationToken);

        return Unit.Default;
    }
}