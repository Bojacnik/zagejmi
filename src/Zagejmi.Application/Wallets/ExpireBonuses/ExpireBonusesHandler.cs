using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Zagejmi.Application.Wallets.Abstractions;
using Zagejmi.Domain.Wallets;

namespace Zagejmi.Application.Wallets.ExpireBonuses;

/// <summary>
///     Returns the unspent rest of every expired bonus grant to the promo budget.
/// </summary>
public sealed partial class ExpireBonusesHandler
{
    /// <summary>
    ///     How many grants are processed per database round trip.
    /// </summary>
    public const int BatchSize = 100;

    private readonly IWalletRepository walletRepository;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<ExpireBonusesHandler> logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExpireBonusesHandler" /> class.
    /// </summary>
    /// <param name="walletRepository">The wallet repository.</param>
    /// <param name="timeProvider">The clock.</param>
    /// <param name="logger">The logger.</param>
    public ExpireBonusesHandler(
        IWalletRepository walletRepository,
        TimeProvider timeProvider,
        ILogger<ExpireBonusesHandler> logger)
    {
        this.walletRepository = walletRepository;
        this.timeProvider = timeProvider;
        this.logger = logger;
    }

    /// <summary>
    ///     Expires all grants that are due.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>How many grants were expired.</returns>
    public async Task<int> Handle(CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = this.timeProvider.GetUtcNow();
        HashSet<Guid> skipped = [];
        int expired = 0;

        while (true)
        {
            IReadOnlyList<BonusGrant> due = await this.walletRepository.GetExpiredGrantsAsync(
                now,
                BatchSize + skipped.Count,
                cancellationToken);

            bool progressed = false;
            foreach (BonusGrant grant in due)
            {
                if (skipped.Contains(grant.Id))
                {
                    continue;
                }

                Account customerBonus = await this.walletRepository.GetOrOpenAccountAsync(
                    grant.CustomerProfileId,
                    AccountKind.CustomerBonus,
                    cancellationToken);

                LedgerTransaction transaction = LedgerTransaction.BonusExpiry(grant, customerBonus, now);
                PostOutcome outcome = await this.walletRepository.PostAsync(transaction, null, cancellationToken);

                if (outcome == PostOutcome.Posted)
                {
                    expired++;
                    progressed = true;
                }
                else
                {
                    // The grant changed in the meantime (or this exact amount already expired); the next run retries it.
                    skipped.Add(grant.Id);
                    this.LogSkipped(grant.Id, outcome);
                }
            }

            if (!progressed)
            {
                return expired;
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Skipped expiring bonus grant {GrantId}: {Outcome}.")]
    private partial void LogSkipped(Guid grantId, PostOutcome outcome);
}
