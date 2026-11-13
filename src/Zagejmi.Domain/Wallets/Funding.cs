namespace Zagejmi.Domain.Wallets;

/// <summary>
///     Where Goins originally came from. Every ledger entry carries it, so paid and bonus Goins stay separately
///     traceable through every step (escrow, payout, fee, refund), even when one payment mixes both.
/// </summary>
public enum Funding
{
    /// <summary>
    ///     Goins backed by real money (a top-up, or player earnings moved to the customer wallet).
    /// </summary>
    Paid,

    /// <summary>
    ///     Free reward Goins funded by the platform's promo budget. They expire and can never be cashed out by the
    ///     customer.
    /// </summary>
    Bonus,
}
