using System;

using Zagejmi.Application.Orders;
using Zagejmi.Domain.Orders;

namespace Zagejmi.Api.Models;

/// <summary>
///     A button pressed on an order card.
/// </summary>
/// <param name="OrderId">The order.</param>
/// <param name="Action">The action.</param>
/// <param name="Outcome">For finishing: how it ended.</param>
/// <param name="Reason">For reporting: what went wrong.</param>
public sealed record OrderActionRequest(Guid OrderId, OrderAction Action, OrderOutcome? Outcome, string? Reason);
