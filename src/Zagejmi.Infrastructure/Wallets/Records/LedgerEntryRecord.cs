using System;

using Zagejmi.Domain.Wallets;

namespace Zagejmi.Infrastructure.Wallets.Records;

/// <summary>
///     Database row of a ledger entry (table <c>wallets.LedgerEntries</c>). Rows are never updated or deleted.
/// </summary>
public record LedgerEntryRecord
{
    /// <summary>
    ///     Gets the entry ID.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    ///     Gets the transaction the entry belongs to.
    /// </summary>
    public Guid TransactionId { get; init; }

    /// <summary>
    ///     Gets the account whose balance changes.
    /// </summary>
    public Guid AccountId { get; init; }

    /// <summary>
    ///     Gets the kind of that account.
    /// </summary>
    public AccountKind AccountKind { get; init; }

    /// <summary>
    ///     Gets the signed change.
    /// </summary>
    public long Amount { get; init; }

    /// <summary>
    ///     Gets whether the Goins are paid or bonus Goins.
    /// </summary>
    public Funding Funding { get; init; }

    /// <summary>
    ///     Gets the bonus grant the Goins come from (bonus entries only).
    /// </summary>
    public Guid? BonusGrantId { get; init; }
}
