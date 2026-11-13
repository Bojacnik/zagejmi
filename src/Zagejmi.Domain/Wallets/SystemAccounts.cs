using System;

namespace Zagejmi.Domain.Wallets;

/// <summary>
///     The fixed identifiers of the platform's system accounts. They are created by a database migration.
/// </summary>
public static class SystemAccounts
{
    /// <summary>
    ///     The <see cref="AccountKind.PromoBudget" /> account.
    /// </summary>
    public static readonly Guid PromoBudgetId = new("00000000-0000-7000-8000-000000000001");

    /// <summary>
    ///     The <see cref="AccountKind.PlatformRevenue" /> account.
    /// </summary>
    public static readonly Guid PlatformRevenueId = new("00000000-0000-7000-8000-000000000002");

    /// <summary>
    ///     The <see cref="AccountKind.Escrow" /> account.
    /// </summary>
    public static readonly Guid EscrowId = new("00000000-0000-7000-8000-000000000003");

    /// <summary>
    ///     The <see cref="AccountKind.TopUpClearing" /> account.
    /// </summary>
    public static readonly Guid TopUpClearingId = new("00000000-0000-7000-8000-000000000004");

    /// <summary>
    ///     The <see cref="AccountKind.PayoutClearing" /> account.
    /// </summary>
    public static readonly Guid PayoutClearingId = new("00000000-0000-7000-8000-000000000005");

    /// <summary>
    ///     Gets the fixed identifier of a system account.
    /// </summary>
    /// <param name="kind">A system account kind.</param>
    /// <returns>The account identifier.</returns>
    /// <exception cref="ArgumentException">Thrown for a user account kind.</exception>
    public static Guid IdOf(AccountKind kind)
    {
        return kind switch
        {
            AccountKind.PromoBudget => PromoBudgetId,
            AccountKind.PlatformRevenue => PlatformRevenueId,
            AccountKind.Escrow => EscrowId,
            AccountKind.TopUpClearing => TopUpClearingId,
            AccountKind.PayoutClearing => PayoutClearingId,
            _ => throw new ArgumentException($"{kind} is not a system account kind.", nameof(kind)),
        };
    }
}
