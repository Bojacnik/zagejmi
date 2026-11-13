using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Wallets.Abstractions;
using Zagejmi.Domain.Wallets;
using Zagejmi.Shared;

namespace Zagejmi.Application.Wallets.GrantReward;

/// <summary>
///     Grants a batch of bonus Goins from the promo budget to a Zákazník wallet.
/// </summary>
public sealed class GrantRewardHandler : IHandlerRequest<GrantRewardCommand, Result<bool>>
{
    private readonly IWalletRepository walletRepository;
    private readonly TimeProvider timeProvider;
    private readonly WalletOptions options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GrantRewardHandler" /> class.
    /// </summary>
    /// <param name="walletRepository">The wallet repository.</param>
    /// <param name="timeProvider">The clock.</param>
    /// <param name="options">The wallet rules.</param>
    public GrantRewardHandler(
        IWalletRepository walletRepository,
        TimeProvider timeProvider,
        IOptions<WalletOptions> options)
    {
        this.walletRepository = walletRepository;
        this.timeProvider = timeProvider;
        this.options = options.Value;
    }

    /// <summary>
    ///     Grants the reward unless it was already granted.
    /// </summary>
    /// <param name="request">The command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     <see langword="true" /> when the Goins were granted now, <see langword="false" /> when this reward had already
    ///     been granted before, or a validation failure.
    /// </returns>
    public async Task<Result<bool>> Handle(GrantRewardCommand request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            return Result<bool>.Failure(ErrorDetail.Validation("Wallet.RewardAmount", "Bonus musí být alespoň 1 Goin."));
        }

        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length > BonusGrant.ReasonMaxLength)
        {
            return Result<bool>.Failure(ErrorDetail.Validation(
                "Wallet.RewardReason",
                $"Důvod bonusu musí mít 1 až {BonusGrant.ReasonMaxLength} znaků."));
        }

        Account customerBonus = await this.walletRepository.GetOrOpenAccountAsync(
            request.CustomerProfileId,
            AccountKind.CustomerBonus,
            cancellationToken);

        BonusGrant grant = BonusGrant.Create(
            request.CustomerProfileId,
            new Goin(request.Amount),
            request.Reason,
            this.timeProvider.GetUtcNow(),
            this.options.BonusLifetime);

        LedgerTransaction transaction = LedgerTransaction.Reward(grant, customerBonus, request.IdempotencyKey);

        PostOutcome outcome = await this.walletRepository.PostAsync(transaction, grant, cancellationToken);

        return outcome switch
        {
            PostOutcome.Posted => true,
            PostOutcome.AlreadyPosted => false,

            // Crediting a wallet from the promo budget cannot run out of funds.
            _ => throw new InvalidOperationException($"Unexpected outcome {outcome} when granting a reward."),
        };
    }
}
