using System;

using Zagejmi.Domain.Orders;

namespace Zagejmi.Application.Orders.ChangeOrder;

/// <summary>
///     The customer or the player acts on an order.
/// </summary>
/// <param name="UserId">The signed-in user.</param>
/// <param name="OrderId">The order.</param>
/// <param name="Action">What to do.</param>
/// <param name="Outcome">For <see cref="OrderAction.Finish" />: how the order ended.</param>
/// <param name="Reason">For <see cref="OrderAction.Report" />: what went wrong.</param>
public sealed record ChangeOrderCommand(
    Guid UserId,
    Guid OrderId,
    OrderAction Action,
    OrderOutcome? Outcome = null,
    string? Reason = null);
