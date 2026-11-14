namespace Zagejmi.Domain.Orders;

/// <summary>
///     Where an order is in its life.
/// </summary>
/// <remarks>
///     Pending → Active → Finished → Completed is the normal path. Pending can end as Cancelled (customer cancels, the
///     player declines, or nobody accepts in time). Active and Finished can be reported and become Disputed, which an
///     admin resolves to Completed (payout) or Cancelled (refund).
/// </remarks>
public enum OrderStatus
{
    /// <summary>
    ///     Paid (Goins in escrow), waiting for the player to accept.
    /// </summary>
    Pending,

    /// <summary>
    ///     Accepted; the player is providing the service.
    /// </summary>
    Active,

    /// <summary>
    ///     The player ended it; waiting for the customer to confirm or report a problem.
    /// </summary>
    Finished,

    /// <summary>
    ///     The customer reported a problem; the Goins stay in escrow until an admin decides.
    /// </summary>
    Disputed,

    /// <summary>
    ///     Done; the player has been paid.
    /// </summary>
    Completed,

    /// <summary>
    ///     Ended without the service; the customer has been refunded.
    /// </summary>
    Cancelled,
}

/// <summary>
///     How the player ended an order.
/// </summary>
public enum OrderOutcome
{
    /// <summary>
    ///     The service was provided.
    /// </summary>
    Delivered,

    /// <summary>
    ///     The customer did not show up; the player is paid anyway because the time was reserved.
    /// </summary>
    CustomerNoShow,
}

/// <summary>
///     Why an order was cancelled (and refunded).
/// </summary>
public enum OrderCancellation
{
    /// <summary>
    ///     The customer cancelled before the player accepted.
    /// </summary>
    CustomerCancelled,

    /// <summary>
    ///     The player declined.
    /// </summary>
    PlayerDeclined,

    /// <summary>
    ///     The player did not accept in time.
    /// </summary>
    AcceptTimeout,

    /// <summary>
    ///     An admin decided a reported order in the customer's favor.
    /// </summary>
    AdminRefunded,
}
