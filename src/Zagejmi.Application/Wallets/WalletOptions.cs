using System;

using Zagejmi.Domain.Wallets;

namespace Zagejmi.Application.Wallets;

/// <summary>
///     Configurable wallet rules (configuration section <c>Wallets</c>).
/// </summary>
public sealed class WalletOptions
{
    /// <summary>
    ///     The configuration section name.
    /// </summary>
    public const string SectionName = "Wallets";

    /// <summary>
    ///     Gets or sets how many bonus Goins a user gets after verifying their email. 0 turns the bonus off.
    /// </summary>
    public long SignUpBonus { get; set; } = 50;

    /// <summary>
    ///     Gets or sets how long bonus Goins stay usable.
    /// </summary>
    public TimeSpan BonusLifetime { get; set; } = BonusGrant.DefaultLifetime;

    /// <summary>
    ///     Gets or sets how often expired bonuses are collected.
    /// </summary>
    public TimeSpan BonusExpiryInterval { get; set; } = TimeSpan.FromHours(1);
}
