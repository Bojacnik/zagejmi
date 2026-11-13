using System;
using System.Collections.Generic;

using Zagejmi.Domain.Wallets;

namespace Zagejmi.Infrastructure.Wallets.Records;

/// <summary>
///     Database row of a ledger transaction (table <c>wallets.LedgerTransactions</c>). Rows are never updated or deleted.
/// </summary>
public record LedgerTransactionRecord
{
    /// <summary>
    ///     Gets the transaction ID.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     Gets the kind of operation.
    /// </summary>
    public LedgerTransactionType Type { get; init; }

    /// <summary>
    ///     Gets when it happened.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    ///     Gets the Czech description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    ///     Gets the key that is unique per transaction type.
    /// </summary>
    public required string IdempotencyKey { get; init; }

    /// <summary>
    ///     Gets the entries.
    /// </summary>
    public List<LedgerEntryRecord> Entries { get; init; } = [];
}
