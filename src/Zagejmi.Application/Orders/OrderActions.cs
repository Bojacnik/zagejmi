using System;
using System.Collections.Generic;

using Zagejmi.Domain.Orders;

namespace Zagejmi.Application.Orders;

/// <summary>
///     Which side of an order a user is on.
/// </summary>
public enum OrderRole
{
    /// <summary>
    ///     The customer who ordered.
    /// </summary>
    Customer,

    /// <summary>
    ///     The player providing the service.
    /// </summary>
    Player,
}

/// <summary>
///     What a customer or player can do with an order.
/// </summary>
public enum OrderAction
{
    /// <summary>Customer: cancel before the player accepted (full refund).</summary>
    Cancel,

    /// <summary>Player: accept a new order.</summary>
    Accept,

    /// <summary>Player: decline a new order (full refund).</summary>
    Decline,

    /// <summary>Player: end an accepted order with an outcome.</summary>
    Finish,

    /// <summary>Customer: confirm a finished order (pays the player).</summary>
    Confirm,

    /// <summary>Customer: report a problem (freezes the money until an admin decides).</summary>
    Report,
}

/// <summary>
///     The rules of who may do what with an order and when. The UI shows exactly these buttons, and the handler
///     refuses anything else.
/// </summary>
public static class OrderActions
{
    /// <summary>
    ///     Gets the actions available to one side of the order right now.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="role">The viewer's side.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The available actions.</returns>
    public static IReadOnlyList<OrderAction> Available(Order order, OrderRole role, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(order);
        return (role, order.Status) switch
        {
            (OrderRole.Customer, OrderStatus.Pending) => [OrderAction.Cancel],
            (OrderRole.Customer, OrderStatus.Active) => [OrderAction.Report],
            (OrderRole.Customer, OrderStatus.Finished) => [OrderAction.Confirm, OrderAction.Report],
            (OrderRole.Player, OrderStatus.Pending) when now < order.AcceptDeadline => [OrderAction.Accept, OrderAction.Decline],
            (OrderRole.Player, OrderStatus.Pending) => [OrderAction.Decline],
            (OrderRole.Player, OrderStatus.Active) => [OrderAction.Finish],
            _ => [],
        };
    }

    /// <summary>
    ///     Gets the side that performs an action.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <returns>The customer or the player.</returns>
    public static OrderRole RoleOf(OrderAction action) => action switch
    {
        OrderAction.Cancel or OrderAction.Confirm or OrderAction.Report => OrderRole.Customer,
        _ => OrderRole.Player,
    };
}
