using System;

namespace Zagejmi.Application.Orders.PlaceOrder;

/// <summary>
///     A customer orders a service.
/// </summary>
/// <param name="UserId">The signed-in user; the order is placed by their customer profile.</param>
/// <param name="ServiceId">The service.</param>
/// <param name="Quantity">How many units (1–10).</param>
public sealed record PlaceOrderCommand(Guid UserId, Guid ServiceId, int Quantity);
