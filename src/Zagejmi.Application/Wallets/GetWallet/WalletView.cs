using System;
using System.Collections.Generic;

using Zagejmi.Domain.Profiles;

namespace Zagejmi.Application.Wallets.GetWallet;

/// <summary>
///     The balance of one wallet.
/// </summary>
/// <param name="Profile">Whose wallet it is (Zákazník or Hráč).</param>
/// <param name="ProfileId">The owning profile.</param>
/// <param name="PaidBalance">Paid Goins (for a Hráč: all earnings).</param>
/// <param name="BonusBalance">Bonus Goins (always 0 for a Hráč).</param>
/// <param name="ActiveBonuses">The unexpired bonus batches, soonest expiry first.</param>
public sealed record WalletView(
    ProfileType Profile,
    Guid ProfileId,
    long PaidBalance,
    long BonusBalance,
    IReadOnlyList<BonusBatchView> ActiveBonuses)
{
    /// <summary>
    ///     Gets the total spendable balance.
    /// </summary>
    public long Balance => this.PaidBalance + this.BonusBalance;
}

/// <summary>
///     One unexpired batch of bonus Goins.
/// </summary>
/// <param name="Reason">Why it was granted.</param>
/// <param name="Remaining">How many Goins are left.</param>
/// <param name="ExpiresAt">When the rest expires.</param>
public sealed record BonusBatchView(string Reason, long Remaining, DateTimeOffset ExpiresAt);
