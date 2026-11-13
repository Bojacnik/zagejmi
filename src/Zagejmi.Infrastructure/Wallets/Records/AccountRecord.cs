using System;

using Zagejmi.Domain.Wallets;

namespace Zagejmi.Infrastructure.Wallets.Records;

/// <summary>
///     Database row of a ledger account (table <c>wallets.Accounts</c>).
/// </summary>
public record AccountRecord
{
    /// <summary>
    ///     Gets the account ID.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     Gets what the account holds.
    /// </summary>
    public AccountKind Kind { get; init; }

    /// <summary>
    ///     Gets the owning profile, or <see langword="null" /> for system accounts.
    /// </summary>
    public Guid? OwnerProfileId { get; init; }

    /// <summary>
    ///     Gets or sets the balance. It always equals the sum of the account's ledger entries.
    /// </summary>
    public long Balance { get; set; }

    /// <summary>
    ///     Gets a value indicating whether the balance may go negative (system accounts only). A check constraint keeps
    ///     every other account at zero or above.
    /// </summary>
    public bool AllowNegative { get; init; }

    /// <summary>
    ///     Gets when the account was opened.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
}
