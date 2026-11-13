namespace Zagejmi.Application.Wallets.Abstractions;

/// <summary>
///     The result of posting a ledger transaction.
/// </summary>
public enum PostOutcome
{
    /// <summary>
    ///     The transaction was stored and all balances changed.
    /// </summary>
    Posted,

    /// <summary>
    ///     A transaction with the same type and idempotency key already exists; nothing changed.
    /// </summary>
    AlreadyPosted,

    /// <summary>
    ///     A user account or bonus grant would have gone negative (e.g. it changed in the meantime); nothing changed.
    /// </summary>
    InsufficientFunds,
}
