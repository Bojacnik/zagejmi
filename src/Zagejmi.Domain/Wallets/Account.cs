using System;

namespace Zagejmi.Domain.Wallets;

/// <summary>
///     One ledger account: a single bucket of Goins. A Zákazník wallet consists of a <see cref="AccountKind.CustomerPaid" />
///     and a <see cref="AccountKind.CustomerBonus" /> account, a Hráč wallet of one
///     <see cref="AccountKind.PlayerEarnings" /> account. The balance only ever changes by posting a
///     <see cref="LedgerTransaction" />.
/// </summary>
public sealed class Account
{
    private Account(Guid id, AccountKind kind, Guid? ownerProfileId, long balance)
    {
        this.Id = id;
        this.Kind = kind;
        this.OwnerProfileId = ownerProfileId;
        this.Balance = balance;
    }

    /// <summary>
    ///     Gets the account identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    ///     Gets what the account holds.
    /// </summary>
    public AccountKind Kind { get; }

    /// <summary>
    ///     Gets the owning profile (customer or player profile ID), or <see langword="null" /> for system accounts.
    /// </summary>
    public Guid? OwnerProfileId { get; }

    /// <summary>
    ///     Gets the balance. Only system accounts can be negative.
    /// </summary>
    public long Balance { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether the account belongs to a user profile (and therefore can never go negative).
    /// </summary>
    public bool IsUserAccount => IsUserKind(this.Kind);

    /// <summary>
    ///     Creates a new, empty account for a profile.
    /// </summary>
    /// <param name="kind">A user account kind.</param>
    /// <param name="ownerProfileId">The owning profile.</param>
    /// <returns>The new account.</returns>
    /// <exception cref="ArgumentException">Thrown for a system account kind.</exception>
    public static Account OpenForProfile(AccountKind kind, Guid ownerProfileId)
    {
        if (!IsUserKind(kind))
        {
            throw new ArgumentException($"{kind} is a system account kind.", nameof(kind));
        }

        return new Account(Guid.CreateVersion7(), kind, ownerProfileId, 0);
    }

    /// <summary>
    ///     Recreates an account from storage.
    /// </summary>
    /// <param name="id">The account identifier.</param>
    /// <param name="kind">The account kind.</param>
    /// <param name="ownerProfileId">The owning profile, or <see langword="null" /> for system accounts.</param>
    /// <param name="balance">The stored balance.</param>
    /// <returns>The account.</returns>
    public static Account Restore(Guid id, AccountKind kind, Guid? ownerProfileId, long balance)
    {
        return new Account(id, kind, ownerProfileId, balance);
    }

    /// <summary>
    ///     Gets a value indicating whether accounts of this kind belong to a user profile.
    /// </summary>
    /// <param name="kind">The account kind.</param>
    /// <returns><see langword="true" /> for customer and player accounts.</returns>
    public static bool IsUserKind(AccountKind kind)
    {
        return kind is AccountKind.CustomerPaid or AccountKind.CustomerBonus or AccountKind.PlayerEarnings;
    }

    /// <summary>
    ///     Applies one ledger entry to the in-memory balance.
    /// </summary>
    /// <param name="entry">The entry; must belong to this account.</param>
    /// <exception cref="ArgumentException">Thrown when the entry belongs to another account.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a user account would go negative.</exception>
    public void Apply(LedgerEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (entry.AccountId != this.Id)
        {
            throw new ArgumentException("The entry belongs to another account.", nameof(entry));
        }

        long newBalance = checked(this.Balance + entry.Amount);
        if (this.IsUserAccount && newBalance < 0)
        {
            throw new InvalidOperationException($"Account {this.Id} ({this.Kind}) cannot go negative.");
        }

        this.Balance = newBalance;
    }
}
