using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Domain.Orders;
using Zagejmi.Domain.Wallets;

namespace Zagejmi.Application.Orders.Abstractions;

/// <summary>
///     Result of storing an order change together with its money movement.
/// </summary>
public enum OrderWriteOutcome
{
    /// <summary>
    ///     Stored.
    /// </summary>
    Saved,

    /// <summary>
    ///     Someone changed the order first (e.g. the player accepted while the customer cancelled); nothing was stored.
    /// </summary>
    StateChanged,

    /// <summary>
    ///     A wallet did not have enough Goins; nothing was stored.
    /// </summary>
    InsufficientFunds,
}

/// <summary>
///     Persists orders. An order change and its money movement are stored atomically: either both or neither.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    ///     Gets an order.
    /// </summary>
    /// <param name="orderId">The order.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The order, or <see langword="null" />.</returns>
    Task<Order?> GetAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Stores a new order together with its payment into escrow.
    /// </summary>
    /// <param name="order">The new order.</param>
    /// <param name="payment">The payment, or <see langword="null" /> for a free order.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Whether it was stored.</returns>
    Task<OrderWriteOutcome> PlaceAsync(Order order, LedgerTransaction? payment, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Stores a state change of an order, only if it is still in <paramref name="previousStatus" />, together with the
    ///     money it moves.
    /// </summary>
    /// <param name="order">The changed order.</param>
    /// <param name="previousStatus">The state the change was made from.</param>
    /// <param name="money">The payout or refund, or <see langword="null" />.</param>
    /// <param name="refundedBonusUsableUntil">
    ///     For a refund: bonus grants that receive Goins back and would expire earlier stay usable until this time.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Whether it was stored.</returns>
    Task<OrderWriteOutcome> SaveTransitionAsync(
        Order order,
        OrderStatus previousStatus,
        LedgerTransaction? money,
        DateTimeOffset? refundedBonusUsableUntil,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets what an order's escrow holds, read from its payment in the ledger.
    /// </summary>
    /// <param name="orderId">The order.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The escrowed Goins (empty for a free order).</returns>
    Task<EscrowFunding> GetEscrowFundingAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a customer's orders, newest first.
    /// </summary>
    /// <param name="customerProfileId">The customer profile.</param>
    /// <param name="skip">How many to skip.</param>
    /// <param name="take">How many to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The page and the total count.</returns>
    Task<PagedResult<Order>> ListForCustomerAsync(
        Guid customerProfileId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the orders of a player's services, newest first.
    /// </summary>
    /// <param name="playerProfileId">The player profile.</param>
    /// <param name="skip">How many to skip.</param>
    /// <param name="take">How many to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The page and the total count.</returns>
    Task<PagedResult<Order>> ListForPlayerAsync(
        Guid playerProfileId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets orders whose deadline has passed: pending past the accept deadline, finished past the confirm deadline.
    /// </summary>
    /// <param name="now">The current time.</param>
    /// <param name="take">The most to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The due orders, oldest deadline first.</returns>
    Task<IReadOnlyList<Order>> GetDueAsync(DateTimeOffset now, int take, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the orders waiting for an admin decision, oldest report first.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The disputed orders.</returns>
    Task<IReadOnlyList<Order>> GetDisputedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a value indicating whether the customer has at least one completed order with the player in which the
    ///     service was actually provided (what a recommendation requires).
    /// </summary>
    /// <param name="customerProfileId">The customer profile.</param>
    /// <param name="playerProfileId">The player profile.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true" /> when such an order exists.</returns>
    Task<bool> HasReviewableOrderAsync(
        Guid customerProfileId,
        Guid playerProfileId,
        CancellationToken cancellationToken = default);
}
