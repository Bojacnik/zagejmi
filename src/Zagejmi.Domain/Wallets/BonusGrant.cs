using System;

namespace Zagejmi.Domain.Wallets;

/// <summary>
///     One batch of bonus Goins granted to a Zákazník wallet. Each batch has its own expiry; spending uses up the batches
///     that expire soonest first, and whatever is left when a batch expires goes back to the promo budget. The sum of
///     <see cref="Remaining" /> over a profile's grants always equals its <see cref="AccountKind.CustomerBonus" />
///     balance.
/// </summary>
public sealed class BonusGrant
{
    /// <summary>
    ///     The longest allowed reason.
    /// </summary>
    public const int ReasonMaxLength = 200;

    /// <summary>
    ///     How long bonus Goins stay usable unless stated otherwise.
    /// </summary>
    public static readonly TimeSpan DefaultLifetime = TimeSpan.FromDays(90);

    private BonusGrant(
        Guid id,
        Guid customerProfileId,
        Goin amount,
        Goin remaining,
        string reason,
        DateTimeOffset grantedAt,
        DateTimeOffset expiresAt)
    {
        this.Id = id;
        this.CustomerProfileId = customerProfileId;
        this.Amount = amount;
        this.Remaining = remaining;
        this.Reason = reason;
        this.GrantedAt = grantedAt;
        this.ExpiresAt = expiresAt;
    }

    /// <summary>
    ///     Gets the grant identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    ///     Gets the customer profile that received the bonus.
    /// </summary>
    public Guid CustomerProfileId { get; }

    /// <summary>
    ///     Gets how many Goins were granted.
    /// </summary>
    public Goin Amount { get; }

    /// <summary>
    ///     Gets how many of them are still unspent and unexpired.
    /// </summary>
    public Goin Remaining { get; private set; }

    /// <summary>
    ///     Gets why the bonus was granted (shown to the user, e.g. „Bonus za registraci“).
    /// </summary>
    public string Reason { get; }

    /// <summary>
    ///     Gets when the bonus was granted.
    /// </summary>
    public DateTimeOffset GrantedAt { get; }

    /// <summary>
    ///     Gets when the unspent rest expires.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; }

    /// <summary>
    ///     Creates a new grant.
    /// </summary>
    /// <param name="customerProfileId">The receiving customer profile.</param>
    /// <param name="amount">The amount; must be positive.</param>
    /// <param name="reason">Why the bonus is granted.</param>
    /// <param name="now">The current time.</param>
    /// <param name="lifetime">How long the bonus stays usable.</param>
    /// <returns>The new grant.</returns>
    public static BonusGrant Create(
        Guid customerProfileId,
        Goin amount,
        string reason,
        DateTimeOffset now,
        TimeSpan lifetime)
    {
        if (amount.IsZero)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "A bonus must be at least 1 Goin.");
        }

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(lifetime, TimeSpan.Zero);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        string trimmed = reason.Trim();
        if (trimmed.Length > ReasonMaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(reason), $"The reason can have at most {ReasonMaxLength} characters.");
        }

        return new BonusGrant(Guid.CreateVersion7(), customerProfileId, amount, amount, trimmed, now, now + lifetime);
    }

    /// <summary>
    ///     Recreates a grant from storage.
    /// </summary>
    /// <param name="id">The grant identifier.</param>
    /// <param name="customerProfileId">The receiving customer profile.</param>
    /// <param name="amount">The granted amount.</param>
    /// <param name="remaining">The unspent amount.</param>
    /// <param name="reason">Why the bonus was granted.</param>
    /// <param name="grantedAt">When it was granted.</param>
    /// <param name="expiresAt">When it expires.</param>
    /// <returns>The grant.</returns>
    public static BonusGrant Restore(
        Guid id,
        Guid customerProfileId,
        Goin amount,
        Goin remaining,
        string reason,
        DateTimeOffset grantedAt,
        DateTimeOffset expiresAt)
    {
        return new BonusGrant(id, customerProfileId, amount, remaining, reason, grantedAt, expiresAt);
    }

    /// <summary>
    ///     Gets a value indicating whether the grant has expired at the given time.
    /// </summary>
    /// <param name="now">The current time.</param>
    /// <returns><see langword="true" /> once <see cref="ExpiresAt" /> has passed.</returns>
    public bool IsExpired(DateTimeOffset now)
    {
        return now >= this.ExpiresAt;
    }

    /// <summary>
    ///     Applies a ledger entry that moves Goins into or out of this grant.
    /// </summary>
    /// <param name="entry">An entry on the customer bonus account that references this grant.</param>
    /// <exception cref="ArgumentException">Thrown when the entry references another grant.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the remaining amount would leave 0..Amount.</exception>
    public void Apply(LedgerEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (entry.BonusGrantId != this.Id || entry.AccountKind != AccountKind.CustomerBonus)
        {
            throw new ArgumentException("The entry does not belong to this grant's bonus account.", nameof(entry));
        }

        long remaining = checked(this.Remaining.Amount + entry.Amount);
        if (remaining < 0 || remaining > this.Amount.Amount)
        {
            throw new InvalidOperationException($"Bonus grant {this.Id} cannot have {remaining} remaining.");
        }

        this.Remaining = new Goin(remaining);
    }
}
