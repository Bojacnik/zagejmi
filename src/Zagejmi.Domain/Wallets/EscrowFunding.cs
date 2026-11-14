using System;
using System.Collections.Generic;
using System.Linq;

namespace Zagejmi.Domain.Wallets;

/// <summary>
///     What an order's escrow holds: paid Goins and bonus Goins per bonus grant. A payout or refund moves exactly this.
/// </summary>
/// <param name="Paid">Paid Goins.</param>
/// <param name="Bonus">Bonus Goins per grant (each part positive).</param>
public sealed record EscrowFunding(long Paid, IReadOnlyList<BonusPart> Bonus)
{
    /// <summary>
    ///     Nothing in escrow (a free order).
    /// </summary>
    public static readonly EscrowFunding Empty = new(0, []);

    /// <summary>
    ///     Gets the total bonus Goins.
    /// </summary>
    public long BonusTotal => this.Bonus.Sum(b => b.Amount);

    /// <summary>
    ///     Gets everything held.
    /// </summary>
    public long Total => this.Paid + this.BonusTotal;

    /// <summary>
    ///     Splits a price into bonus and paid Goins: bonus first, from the grants that expire soonest, then paid.
    /// </summary>
    /// <param name="total">The price.</param>
    /// <param name="activeGrants">The customer's unexpired grants with something remaining.</param>
    /// <returns>The split; <see cref="Paid" /> may exceed the customer's paid balance, the ledger then refuses it.</returns>
    public static EscrowFunding Split(long total, IEnumerable<BonusGrant> activeGrants)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(total);
        List<BonusPart> parts = [];
        long left = total;
        foreach (BonusGrant grant in activeGrants.OrderBy(g => g.ExpiresAt).ThenBy(g => g.Id))
        {
            if (left == 0)
            {
                break;
            }

            long take = Math.Min(left, grant.Remaining.Amount);
            if (take > 0)
            {
                parts.Add(new BonusPart(grant.Id, take));
                left -= take;
            }
        }

        return new EscrowFunding(left, parts);
    }
}

/// <summary>
///     Bonus Goins from one grant.
/// </summary>
/// <param name="GrantId">The bonus grant.</param>
/// <param name="Amount">The amount (positive).</param>
public sealed record BonusPart(Guid GrantId, long Amount);
