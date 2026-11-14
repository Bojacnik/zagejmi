using System;

using Zagejmi.Domain.Orders;
using Zagejmi.Shared;

namespace Zagejmi.Application.Orders;

/// <summary>
///     Errors shared by the order use cases.
/// </summary>
internal static class OrderErrors
{
    /// <summary>
    ///     Ghosts can only browse.
    /// </summary>
    public static readonly ErrorDetail GhostMode = new(
        "User.GhostMode",
        "V režimu ducha můžete web pouze prohlížet. Pro objednávky režim ducha vypněte.",
        ErrorType.Forbidden);

    /// <summary>
    ///     Someone else changed the order first.
    /// </summary>
    public static readonly ErrorDetail Changed = new(
        "Order.Changed",
        "Objednávka se mezitím změnila. Načtěte ji prosím znovu.",
        ErrorType.Conflict);

    /// <summary>
    ///     The user is not on the side of the order the action needs.
    /// </summary>
    public static readonly ErrorDetail NotYours = new(
        "Order.NotYours",
        "K této objednávce nemáte přístup.",
        ErrorType.Forbidden);

    /// <summary>
    ///     The order does not exist (or the user may not see it).
    /// </summary>
    /// <param name="orderId">The order.</param>
    /// <returns>The error.</returns>
    public static ErrorDetail NotFound(Guid orderId) => ErrorDetail.NotFound(nameof(Order), orderId);

    /// <summary>
    ///     The customer cannot pay.
    /// </summary>
    /// <param name="total">The price.</param>
    /// <param name="available">The customer's balance.</param>
    /// <returns>The error.</returns>
    public static ErrorDetail InsufficientFunds(long total, long available) => ErrorDetail.Validation(
        "Order.InsufficientFunds",
        $"Na objednávku potřebujete {Goins(total)}, ale v peněžence máte {Goins(available)}.");

    private static string Goins(long amount)
    {
        long abs = Math.Abs(amount);
        return $"{amount:N0} {(abs == 1 ? "Goin" : abs is >= 2 and <= 4 ? "Goiny" : "Goinů")}";
    }
}
