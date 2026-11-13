namespace Zagejmi.Domain.Wallets;

/// <summary>
///     The kinds of operations that move Goins. There is deliberately no general "transfer": every allowed flow has its
///     own type and its own factory method on <see cref="LedgerTransaction" />.
/// </summary>
public enum LedgerTransactionType
{
    /// <summary>
    ///     Bonus Goins granted to a Zákazník wallet from the promo budget (e.g. the sign-up bonus).
    /// </summary>
    Reward,

    /// <summary>
    ///     The unspent rest of a bonus grant returned to the promo budget after it expired.
    /// </summary>
    BonusExpiry,
}
