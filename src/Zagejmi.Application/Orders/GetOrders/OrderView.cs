using System;
using System.Collections.Generic;

using Zagejmi.Application.Profiles;
using Zagejmi.Domain.Orders;

namespace Zagejmi.Application.Orders.GetOrders;

/// <summary>
///     An order as one of its sides (or an admin) sees it.
/// </summary>
/// <param name="Order">The order.</param>
/// <param name="Customer">The ordering customer.</param>
/// <param name="Player">The player providing the service.</param>
/// <param name="Actions">What the viewer can do with it now.</param>
public sealed record OrderView(Order Order, ProfileCard Customer, ProfileCard Player, IReadOnlyList<OrderAction> Actions);
