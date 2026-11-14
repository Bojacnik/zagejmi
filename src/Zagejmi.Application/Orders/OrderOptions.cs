using System;

namespace Zagejmi.Application.Orders;

/// <summary>
///     Settings of the order flow (configuration section <c>Orders</c>).
/// </summary>
public sealed class OrderOptions
{
    /// <summary>
    ///     The configuration section name.
    /// </summary>
    public const string SectionName = "Orders";

    /// <summary>
    ///     Gets or sets the platform fee in whole percent of the order total (rounded down, at least 1 Goin).
    /// </summary>
    public int FeePercent { get; set; } = 10;

    /// <summary>
    ///     Gets or sets how long the player has to accept a new order before it is cancelled and refunded.
    /// </summary>
    public TimeSpan AcceptWindow { get; set; } = TimeSpan.FromHours(48);

    /// <summary>
    ///     Gets or sets how long the customer has to confirm or report a finished order before it completes by itself.
    /// </summary>
    public TimeSpan ConfirmWindow { get; set; } = TimeSpan.FromHours(72);

    /// <summary>
    ///     Gets or sets the least time refunded bonus Goins stay usable: a grant that has expired (or is about to) while
    ///     its Goins were in escrow gets this long again, so a refund never hands back Goins that vanish at once.
    /// </summary>
    public TimeSpan RefundedBonusGrace { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    ///     Gets or sets how often the timeout job looks for expired orders.
    /// </summary>
    public TimeSpan TimeoutInterval { get; set; } = TimeSpan.FromMinutes(5);
}
