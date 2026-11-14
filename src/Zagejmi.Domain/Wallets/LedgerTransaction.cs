using System;
using System.Collections.Generic;
using System.Linq;

namespace Zagejmi.Domain.Wallets;

/// <summary>
///     One operation that moves Goins, made of balanced <see cref="LedgerEntry" /> lines. Transactions can only be created
///     through the factory methods below, one per allowed flow, so a flow that is not allowed (e.g. sending Goins to
///     someone else's wallet) cannot even be expressed.
/// </summary>
/// <remarks>
///     Invariants checked for every transaction:
///     <list type="bullet">
///         <item>the entries add up to zero separately for paid and for bonus Goins, so the two never mix;</item>
///         <item>bonus entries always name their bonus grant, paid entries never do;</item>
///         <item>the paid part of a customer wallet only ever holds paid Goins, the bonus part only bonus Goins.</item>
///     </list>
/// </remarks>
public sealed class LedgerTransaction
{
    /// <summary>
    ///     The longest allowed description.
    /// </summary>
    public const int DescriptionMaxLength = 300;

    /// <summary>
    ///     The longest allowed idempotency key.
    /// </summary>
    public const int IdempotencyKeyMaxLength = 200;

    private LedgerTransaction(
        LedgerTransactionType type,
        DateTimeOffset createdAt,
        string description,
        string idempotencyKey,
        IReadOnlyList<LedgerEntry> entries)
    {
        Validate(description, idempotencyKey, entries);

        this.Id = Guid.CreateVersion7();
        this.Type = type;
        this.CreatedAt = createdAt;
        this.Description = description;
        this.IdempotencyKey = idempotencyKey;
        this.Entries = entries;
    }

    /// <summary>
    ///     Gets the transaction identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    ///     Gets the kind of operation.
    /// </summary>
    public LedgerTransactionType Type { get; }

    /// <summary>
    ///     Gets when the transaction happened.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    ///     Gets a Czech description shown in the wallet history.
    /// </summary>
    public string Description { get; }

    /// <summary>
    ///     Gets a key that is unique per transaction type, so a retried or double-clicked operation is posted only once.
    /// </summary>
    public string IdempotencyKey { get; }

    /// <summary>
    ///     Gets the balanced entries.
    /// </summary>
    public IReadOnlyList<LedgerEntry> Entries { get; }

    /// <summary>
    ///     Grants bonus Goins from the promo budget to a Zákazník wallet.
    /// </summary>
    /// <param name="grant">The new bonus grant.</param>
    /// <param name="customerBonus">The receiving profile's <see cref="AccountKind.CustomerBonus" /> account.</param>
    /// <param name="idempotencyKey">A key that identifies this reward (e.g. <c>signup-bonus:{userId}</c>).</param>
    /// <returns>The transaction.</returns>
    public static LedgerTransaction Reward(BonusGrant grant, Account customerBonus, string idempotencyKey)
    {
        ArgumentNullException.ThrowIfNull(grant);
        EnsureCustomerBonusOf(customerBonus, grant);

        long amount = grant.Amount.Amount;
        LedgerEntry[] entries =
        [
            new(SystemAccounts.PromoBudgetId, AccountKind.PromoBudget, -amount, Funding.Bonus, grant.Id),
            new(customerBonus.Id, AccountKind.CustomerBonus, amount, Funding.Bonus, grant.Id),
        ];

        return new LedgerTransaction(LedgerTransactionType.Reward, grant.GrantedAt, grant.Reason, idempotencyKey, entries);
    }

    /// <summary>
    ///     Returns the unspent rest of an expired bonus grant to the promo budget.
    /// </summary>
    /// <param name="grant">The expired grant; must still have something remaining.</param>
    /// <param name="customerBonus">The owning profile's <see cref="AccountKind.CustomerBonus" /> account.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The transaction.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the grant has not expired or nothing remains.</exception>
    public static LedgerTransaction BonusExpiry(BonusGrant grant, Account customerBonus, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(grant);
        EnsureCustomerBonusOf(customerBonus, grant);

        if (!grant.IsExpired(now))
        {
            throw new InvalidOperationException($"Bonus grant {grant.Id} has not expired yet.");
        }

        if (grant.Remaining.IsZero)
        {
            throw new InvalidOperationException($"Bonus grant {grant.Id} has nothing left to expire.");
        }

        long amount = grant.Remaining.Amount;
        LedgerEntry[] entries =
        [
            new(customerBonus.Id, AccountKind.CustomerBonus, -amount, Funding.Bonus, grant.Id),
            new(SystemAccounts.PromoBudgetId, AccountKind.PromoBudget, amount, Funding.Bonus, grant.Id),
        ];

        string description = Truncate($"Vypršení bonusu: {grant.Reason}", DescriptionMaxLength);

        // The expiry time is part of the key: if a refund later reopens the grant with a new expiry, it can expire again.
        string idempotencyKey = $"bonus-expiry:{grant.Id}:{grant.ExpiresAt.ToUnixTimeSeconds()}";

        return new LedgerTransaction(LedgerTransactionType.BonusExpiry, now, description, idempotencyKey, entries);
    }

    /// <summary>
    ///     Pays for an order into escrow, bonus Goins first.
    /// </summary>
    /// <param name="orderId">The order.</param>
    /// <param name="serviceName">The ordered service (for the history).</param>
    /// <param name="funding">How the price is split into bonus and paid Goins.</param>
    /// <param name="customerPaid">The customer's <see cref="AccountKind.CustomerPaid" /> account.</param>
    /// <param name="customerBonus">The customer's <see cref="AccountKind.CustomerBonus" /> account.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The transaction.</returns>
    public static LedgerTransaction OrderPayment(
        Guid orderId,
        string serviceName,
        EscrowFunding funding,
        Account customerPaid,
        Account customerBonus,
        DateTimeOffset now)
    {
        EnsureCustomerWallet(customerPaid, customerBonus);
        List<LedgerEntry> entries = [];
        if (funding.Paid > 0)
        {
            entries.Add(new(customerPaid.Id, AccountKind.CustomerPaid, -funding.Paid, Funding.Paid, null));
            entries.Add(new(SystemAccounts.EscrowId, AccountKind.Escrow, funding.Paid, Funding.Paid, null));
        }

        foreach (BonusPart part in funding.Bonus)
        {
            entries.Add(new(customerBonus.Id, AccountKind.CustomerBonus, -part.Amount, Funding.Bonus, part.GrantId));
            entries.Add(new(SystemAccounts.EscrowId, AccountKind.Escrow, part.Amount, Funding.Bonus, part.GrantId));
        }

        return new LedgerTransaction(
            LedgerTransactionType.OrderPayment,
            now,
            Truncate($"Objednávka: {serviceName}", DescriptionMaxLength),
            OrderKey(orderId),
            entries);
    }

    /// <summary>
    ///     Pays a completed order out to the player: the player receives the escrow minus the fee. The fee comes out of
    ///     the paid Goins; bonus Goins go back to the promo budget, which pays the player the rest of their share in paid
    ///     Goins (bonus Goins are never cashable, so the player must not receive them).
    /// </summary>
    /// <param name="orderId">The order.</param>
    /// <param name="serviceName">The ordered service (for the history).</param>
    /// <param name="funding">What the order's escrow holds.</param>
    /// <param name="fee">The platform fee (at most the escrow total).</param>
    /// <param name="playerEarnings">The player's <see cref="AccountKind.PlayerEarnings" /> account.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The transaction.</returns>
    public static LedgerTransaction OrderPayout(
        Guid orderId,
        string serviceName,
        EscrowFunding funding,
        long fee,
        Account playerEarnings,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(funding);
        ArgumentNullException.ThrowIfNull(playerEarnings);
        if (playerEarnings.Kind != AccountKind.PlayerEarnings)
        {
            throw new ArgumentException("The account is not a player earnings account.", nameof(playerEarnings));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(fee);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(fee, funding.Total);

        long share = funding.Total - fee;
        long shareFromPaid = Math.Min(funding.Paid, share);
        long revenue = funding.Paid - shareFromPaid;
        long shareFromPromo = share - shareFromPaid;

        List<LedgerEntry> entries = [];
        AddIfNotZero(entries, new(SystemAccounts.EscrowId, AccountKind.Escrow, -funding.Paid, Funding.Paid, null));
        AddIfNotZero(entries, new(playerEarnings.Id, AccountKind.PlayerEarnings, shareFromPaid, Funding.Paid, null));
        AddIfNotZero(entries, new(SystemAccounts.PlatformRevenueId, AccountKind.PlatformRevenue, revenue, Funding.Paid, null));
        AddIfNotZero(entries, new(SystemAccounts.PromoBudgetId, AccountKind.PromoBudget, -shareFromPromo, Funding.Paid, null));
        AddIfNotZero(entries, new(playerEarnings.Id, AccountKind.PlayerEarnings, shareFromPromo, Funding.Paid, null));
        foreach (BonusPart part in funding.Bonus)
        {
            entries.Add(new(SystemAccounts.EscrowId, AccountKind.Escrow, -part.Amount, Funding.Bonus, part.GrantId));
            entries.Add(new(SystemAccounts.PromoBudgetId, AccountKind.PromoBudget, part.Amount, Funding.Bonus, part.GrantId));
        }

        return new LedgerTransaction(
            LedgerTransactionType.OrderPayout,
            now,
            Truncate($"Výplata objednávky: {serviceName}", DescriptionMaxLength),
            OrderKey(orderId),
            entries);
    }

    /// <summary>
    ///     Returns a cancelled order's escrow to the customer, each part where it came from (bonus Goins to their grant).
    /// </summary>
    /// <param name="orderId">The order.</param>
    /// <param name="serviceName">The ordered service (for the history).</param>
    /// <param name="funding">What the order's escrow holds.</param>
    /// <param name="customerPaid">The customer's <see cref="AccountKind.CustomerPaid" /> account.</param>
    /// <param name="customerBonus">The customer's <see cref="AccountKind.CustomerBonus" /> account.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The transaction.</returns>
    public static LedgerTransaction OrderRefund(
        Guid orderId,
        string serviceName,
        EscrowFunding funding,
        Account customerPaid,
        Account customerBonus,
        DateTimeOffset now)
    {
        EnsureCustomerWallet(customerPaid, customerBonus);
        List<LedgerEntry> entries = [];
        if (funding.Paid > 0)
        {
            entries.Add(new(SystemAccounts.EscrowId, AccountKind.Escrow, -funding.Paid, Funding.Paid, null));
            entries.Add(new(customerPaid.Id, AccountKind.CustomerPaid, funding.Paid, Funding.Paid, null));
        }

        foreach (BonusPart part in funding.Bonus)
        {
            entries.Add(new(SystemAccounts.EscrowId, AccountKind.Escrow, -part.Amount, Funding.Bonus, part.GrantId));
            entries.Add(new(customerBonus.Id, AccountKind.CustomerBonus, part.Amount, Funding.Bonus, part.GrantId));
        }

        return new LedgerTransaction(
            LedgerTransactionType.OrderRefund,
            now,
            Truncate($"Vrácení za objednávku: {serviceName}", DescriptionMaxLength),
            OrderKey(orderId),
            entries);
    }

    private static string OrderKey(Guid orderId) => $"order:{orderId}";

    private static void AddIfNotZero(List<LedgerEntry> entries, LedgerEntry entry)
    {
        if (entry.Amount != 0)
        {
            entries.Add(entry);
        }
    }

    private static void EnsureCustomerWallet(Account customerPaid, Account customerBonus)
    {
        ArgumentNullException.ThrowIfNull(customerPaid);
        ArgumentNullException.ThrowIfNull(customerBonus);
        if (customerPaid.Kind != AccountKind.CustomerPaid
            || customerBonus.Kind != AccountKind.CustomerBonus
            || customerPaid.OwnerProfileId != customerBonus.OwnerProfileId)
        {
            throw new ArgumentException("The accounts are not one customer's paid and bonus accounts.", nameof(customerPaid));
        }
    }

    private static void EnsureCustomerBonusOf(Account customerBonus, BonusGrant grant)
    {
        ArgumentNullException.ThrowIfNull(customerBonus);

        if (customerBonus.Kind != AccountKind.CustomerBonus || customerBonus.OwnerProfileId != grant.CustomerProfileId)
        {
            throw new ArgumentException(
                "The account is not the bonus account of the grant's customer profile.",
                nameof(customerBonus));
        }
    }

    private static void Validate(string description, string idempotencyKey, IReadOnlyList<LedgerEntry> entries)
    {
        if (string.IsNullOrWhiteSpace(description) || description.Length > DescriptionMaxLength)
        {
            throw new ArgumentException($"The description must have 1 to {DescriptionMaxLength} characters.", nameof(description));
        }

        if (string.IsNullOrWhiteSpace(idempotencyKey) || idempotencyKey.Length > IdempotencyKeyMaxLength)
        {
            throw new ArgumentException(
                $"The idempotency key must have 1 to {IdempotencyKeyMaxLength} characters.",
                nameof(idempotencyKey));
        }

        if (entries.Count < 2)
        {
            throw new ArgumentException("A transaction needs at least two entries.", nameof(entries));
        }

        foreach (LedgerEntry entry in entries)
        {
            if (entry.Amount == 0)
            {
                throw new ArgumentException("Ledger entries cannot be zero.", nameof(entries));
            }

            if ((entry.Funding == Funding.Bonus) != entry.BonusGrantId.HasValue)
            {
                throw new ArgumentException("Bonus entries must name their bonus grant and paid entries must not.", nameof(entries));
            }

            if ((entry.AccountKind == AccountKind.CustomerPaid && entry.Funding != Funding.Paid)
                || (entry.AccountKind == AccountKind.CustomerBonus && entry.Funding != Funding.Bonus))
            {
                throw new ArgumentException(
                    "Customer paid accounts only hold paid Goins and bonus accounts only bonus Goins.",
                    nameof(entries));
            }
        }

        foreach (IGrouping<Funding, LedgerEntry> funding in entries.GroupBy(e => e.Funding))
        {
            long sum = funding.Aggregate(0L, (total, e) => checked(total + e.Amount));
            if (sum != 0)
            {
                throw new ArgumentException($"{funding.Key} entries add up to {sum} instead of zero.", nameof(entries));
            }
        }
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
