using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Domain.Wallets;

namespace Zagejmi.Application.Wallets.Abstractions;

/// <summary>
///     Stores ledger accounts, transactions and bonus grants.
/// </summary>
public interface IWalletRepository
{
    /// <summary>
    ///     Gets a profile's account of the given kind, creating an empty one if it does not exist yet. Safe to call
    ///     concurrently: two callers always get the same account.
    /// </summary>
    /// <param name="profileId">The owning customer or player profile.</param>
    /// <param name="kind">A user account kind.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The account.</returns>
    Task<Account> GetOrOpenAccountAsync(Guid profileId, AccountKind kind, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets all accounts a profile owns (none if it never had a wallet).
    /// </summary>
    /// <param name="profileId">The owning profile.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The accounts.</returns>
    Task<IReadOnlyList<Account>> GetAccountsAsync(Guid profileId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Atomically stores the transaction, applies its entries to the account balances and bonus grants, and stores
    ///     <paramref name="newGrant" /> if given. Nothing is written unless everything succeeds.
    /// </summary>
    /// <param name="transaction">The transaction to post.</param>
    /// <param name="newGrant">A bonus grant created by this transaction, or <see langword="null" />.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>What happened.</returns>
    Task<PostOutcome> PostAsync(
        LedgerTransaction transaction,
        BonusGrant? newGrant = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets expired bonus grants that still have Goins remaining, oldest expiry first.
    /// </summary>
    /// <param name="now">The current time.</param>
    /// <param name="take">The most grants to return.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The grants.</returns>
    Task<IReadOnlyList<BonusGrant>> GetExpiredGrantsAsync(
        DateTimeOffset now,
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the bonus grants of a customer profile that still have Goins remaining and have not expired, soonest
    ///     expiry first.
    /// </summary>
    /// <param name="customerProfileId">The customer profile.</param>
    /// <param name="now">The current time.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The grants.</returns>
    Task<IReadOnlyList<BonusGrant>> GetActiveGrantsAsync(
        Guid customerProfileId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a page of the transactions that touched any of the given accounts, newest first, with the net change per
    ///     funding type summed over those accounts.
    /// </summary>
    /// <param name="accountIds">The wallet's accounts.</param>
    /// <param name="skip">How many transactions to skip.</param>
    /// <param name="take">The most transactions to return.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The history items.</returns>
    Task<IReadOnlyList<WalletHistoryItem>> GetHistoryAsync(
        IReadOnlyCollection<Guid> accountIds,
        int skip,
        int take,
        CancellationToken cancellationToken = default);
}
