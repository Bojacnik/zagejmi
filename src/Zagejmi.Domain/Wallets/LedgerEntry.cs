using System;

namespace Zagejmi.Domain.Wallets;

/// <summary>
///     One line of a <see cref="LedgerTransaction" />: a signed change of one account's balance.
/// </summary>
/// <param name="AccountId">The account whose balance changes.</param>
/// <param name="AccountKind">The kind of that account (kept so the entry can be validated and reported on its own).</param>
/// <param name="Amount">The change: positive credits the account, negative debits it. Never zero.</param>
/// <param name="Funding">Whether the Goins are paid or bonus Goins.</param>
/// <param name="BonusGrantId">The bonus grant the Goins come from; set exactly when <paramref name="Funding" /> is bonus.</param>
public sealed record LedgerEntry(
    Guid AccountId,
    AccountKind AccountKind,
    long Amount,
    Funding Funding,
    Guid? BonusGrantId);
