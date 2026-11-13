using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Application.Auth.Users.EmailVerification;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Application.Wallets;
using Zagejmi.Application.Wallets.GrantReward;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Profiles.Customers;
using Zagejmi.Shared;

namespace Zagejmi.Application.Auth.Users.VerifyEmail;

/// <summary>
///     Confirms a user's email address and grants the sign-up bonus to their Zákazník wallet.
/// </summary>
public sealed class VerifyEmailHandler : IHandlerRequest<VerifyEmailCommand, Result<long>>
{
    /// <summary>
    ///     The reason shown in the wallet history.
    /// </summary>
    public const string SignUpBonusReason = "Bonus za registraci";

    private static readonly ErrorDetail InvalidLink = ErrorDetail.Validation(
        "User.VerificationLinkInvalid",
        "Ověřovací odkaz je neplatný nebo mu vypršela platnost. Na přihlašovací stránce si můžete poslat nový.");

    private readonly IUserRepository userRepository;
    private readonly IProfileRepository profileRepository;
    private readonly EmailVerificationMailer mailer;
    private readonly GrantRewardHandler grantRewardHandler;
    private readonly TimeProvider timeProvider;
    private readonly WalletOptions walletOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="VerifyEmailHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="profileRepository">The profile repository.</param>
    /// <param name="mailer">Checks verification tokens.</param>
    /// <param name="grantRewardHandler">Grants the sign-up bonus.</param>
    /// <param name="timeProvider">The clock.</param>
    /// <param name="walletOptions">The wallet rules.</param>
    public VerifyEmailHandler(
        IUserRepository userRepository,
        IProfileRepository profileRepository,
        EmailVerificationMailer mailer,
        GrantRewardHandler grantRewardHandler,
        TimeProvider timeProvider,
        IOptions<WalletOptions> walletOptions)
    {
        this.userRepository = userRepository;
        this.profileRepository = profileRepository;
        this.mailer = mailer;
        this.grantRewardHandler = grantRewardHandler;
        this.timeProvider = timeProvider;
        this.walletOptions = walletOptions.Value;
    }

    /// <summary>
    ///     Verifies the email. Opening the same link again is harmless: an already verified user just gets a success
    ///     (and the sign-up bonus, should granting it have failed the first time).
    /// </summary>
    /// <param name="request">The command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     The sign-up bonus the user has received (now or on an earlier visit of the link; 0 if none), or a failure for an
    ///     invalid link.
    /// </returns>
    public async Task<Result<long>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken = default)
    {
        User? user = await this.userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result<long>.Failure(InvalidLink);
        }

        if (user.AuthCredentials.EmailVerifiedAt is null)
        {
            if (string.IsNullOrWhiteSpace(request.Token) || !this.mailer.IsValid(user, request.Token))
            {
                return Result<long>.Failure(InvalidLink);
            }

            user.AuthCredentials.EmailVerifiedAt = this.timeProvider.GetUtcNow();
            user.AuthCredentials.PendingEmailVerificationTokenHash = null;
            user.AuthCredentials.PendingEmailVerificationExpiresAt = null;
            await this.userRepository.UpdateAsync(user, cancellationToken);
        }

        return await this.GrantSignUpBonusAsync(user, cancellationToken);
    }

    private async Task<Result<long>> GrantSignUpBonusAsync(User user, CancellationToken cancellationToken)
    {
        long amount = this.walletOptions.SignUpBonus;
        if (amount <= 0)
        {
            return 0L;
        }

        // Users and profiles are created in separate steps; a user whose customer profile is missing gets no bonus
        // rather than a failed verification.
        CustomerProfile? customer = await this.profileRepository.GetCustomerByUserIdAsync(user.Id, cancellationToken);
        if (customer is null)
        {
            return 0L;
        }

        Result<bool> granted = await this.grantRewardHandler.Handle(
            new GrantRewardCommand
            {
                CustomerProfileId = customer.Id,
                Amount = amount,
                Reason = SignUpBonusReason,
                IdempotencyKey = $"signup-bonus:{user.Id}",
            },
            cancellationToken);

        if (granted.IsFailure)
        {
            return Result<long>.Failure(granted.Error!);
        }

        // Already granted earlier (e.g. the link was opened twice) still counts: the user has the bonus.
        return amount;
    }
}
