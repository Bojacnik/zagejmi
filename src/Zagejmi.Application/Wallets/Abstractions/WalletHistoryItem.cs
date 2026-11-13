using System;

using Zagejmi.Domain.Wallets;

namespace Zagejmi.Application.Wallets.Abstractions;

/// <summary>
///     One transaction as seen from one wallet.
/// </summary>
/// <param name="TransactionId">The transaction identifier.</param>
/// <param name="Type">The kind of operation.</param>
/// <param name="Description">The Czech description.</param>
/// <param name="CreatedAt">When it happened.</param>
/// <param name="PaidChange">The net change of the wallet in paid Goins.</param>
/// <param name="BonusChange">The net change of the wallet in bonus(-funded) Goins.</param>
public sealed record WalletHistoryItem(
    Guid TransactionId,
    LedgerTransactionType Type,
    string Description,
    DateTimeOffset CreatedAt,
    long PaidChange,
    long BonusChange)
{
    /// <summary>
    ///     Gets the total net change of the wallet.
    /// </summary>
    public long TotalChange => this.PaidChange + this.BonusChange;
}
