namespace Zagejmi.Domain.Wallets;

/// <summary>
///     What a ledger account holds. User accounts belong to one profile and can never go negative; system accounts
///     belong to the platform.
/// </summary>
public enum AccountKind
{
    /// <summary>
    ///     The paid part of a Zákazník wallet (closed loop: can be spent, never cashed out).
    /// </summary>
    CustomerPaid,

    /// <summary>
    ///     The bonus part of a Zákazník wallet. Its balance always equals the remaining amount of the profile's bonus
    ///     grants.
    /// </summary>
    CustomerBonus,

    /// <summary>
    ///     A Hráč wallet: earnings that can be cashed out or moved to the player's own Zákazník wallet.
    /// </summary>
    PlayerEarnings,

    /// <summary>
    ///     System: the source of every bonus Goin. Its (negative) balance is how much the platform has given away.
    /// </summary>
    PromoBudget,

    /// <summary>
    ///     System: fees the platform has earned.
    /// </summary>
    PlatformRevenue,

    /// <summary>
    ///     System: Goins held for open orders.
    /// </summary>
    Escrow,

    /// <summary>
    ///     System: the counterpart of real money coming in through top-ups.
    /// </summary>
    TopUpClearing,

    /// <summary>
    ///     System: the counterpart of real money going out through player cash-outs.
    /// </summary>
    PayoutClearing,
}
