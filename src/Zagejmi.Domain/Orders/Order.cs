using System;

namespace Zagejmi.Domain.Orders;

/// <summary>
///     A customer's order of a player's service. The price is paid into escrow when the order is placed; the
///     transitions below say when it is paid out to the player (<see cref="MoneyMove.Payout" />) or refunded
///     (<see cref="MoneyMove.Refund" />). The service name, unit and price are copied at order time, so later edits of the
///     service don't change past orders.
/// </summary>
public sealed class Order
{
    /// <summary>
    ///     The most units one order can buy.
    /// </summary>
    public const int MaxQuantity = 10;

    /// <summary>
    ///     The longest allowed problem description.
    /// </summary>
    public const int DisputeReasonMaxLength = 1000;

    private Order()
    {
    }

    /// <summary>
    ///     What happens to the escrowed Goins when an order changes state.
    /// </summary>
    public enum MoneyMove
    {
        /// <summary>
        ///     The Goins stay where they are.
        /// </summary>
        None,

        /// <summary>
        ///     Escrow → player (minus the fee).
        /// </summary>
        Payout,

        /// <summary>
        ///     Escrow → back to the customer.
        /// </summary>
        Refund,
    }

    /// <summary>Gets the order ID.</summary>
    public Guid Id { get; private init; }

    /// <summary>Gets the ordering customer profile.</summary>
    public Guid CustomerProfileId { get; private init; }

    /// <summary>Gets the player profile providing the service.</summary>
    public Guid PlayerProfileId { get; private init; }

    /// <summary>Gets the ordered service.</summary>
    public Guid ServiceId { get; private init; }

    /// <summary>Gets the service name at order time.</summary>
    public string ServiceName { get; private init; } = string.Empty;

    /// <summary>Gets the price unit at order time (e.g. "/ hodina"), if any.</summary>
    public string? PriceUnit { get; private init; }

    /// <summary>Gets the price of one unit in Goins at order time.</summary>
    public long UnitPrice { get; private init; }

    /// <summary>Gets how many units were ordered.</summary>
    public int Quantity { get; private init; }

    /// <summary>Gets the total price (<see cref="UnitPrice" /> × <see cref="Quantity" />) held in escrow.</summary>
    public long Total { get; private init; }

    /// <summary>Gets the platform fee, fixed at order time; the player receives <see cref="Total" /> − fee.</summary>
    public long Fee { get; private init; }

    /// <summary>Gets the current state.</summary>
    public OrderStatus Status { get; private set; }

    /// <summary>Gets how the player ended the order, once finished.</summary>
    public OrderOutcome? Outcome { get; private set; }

    /// <summary>Gets why the order was cancelled, once cancelled.</summary>
    public OrderCancellation? Cancellation { get; private set; }

    /// <summary>Gets when the order was placed.</summary>
    public DateTimeOffset CreatedAt { get; private init; }

    /// <summary>Gets until when the player can accept; after that the order is cancelled and refunded.</summary>
    public DateTimeOffset AcceptDeadline { get; private init; }

    /// <summary>Gets when the player accepted.</summary>
    public DateTimeOffset? AcceptedAt { get; private set; }

    /// <summary>Gets when the player ended the order.</summary>
    public DateTimeOffset? FinishedAt { get; private set; }

    /// <summary>Gets until when the customer can confirm or report; after that the order completes by itself.</summary>
    public DateTimeOffset? ConfirmDeadline { get; private set; }

    /// <summary>Gets when the customer reported a problem.</summary>
    public DateTimeOffset? DisputedAt { get; private set; }

    /// <summary>Gets the customer's description of the problem.</summary>
    public string? DisputeReason { get; private set; }

    /// <summary>Gets when the order reached <see cref="OrderStatus.Completed" /> or <see cref="OrderStatus.Cancelled" />.</summary>
    public DateTimeOffset? ClosedAt { get; private set; }

    /// <summary>Gets the admin who resolved the dispute, if any.</summary>
    public Guid? ResolvedByUserId { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether the order lets the customer review the service: it was completed and the
    ///     service was actually provided.
    /// </summary>
    public bool UnlocksReview => this.Status == OrderStatus.Completed && this.Outcome == OrderOutcome.Delivered;

    /// <summary>
    ///     Gets the amount the player receives on payout.
    /// </summary>
    public long PlayerShare => this.Total - this.Fee;

    /// <summary>
    ///     Computes the platform fee: <paramref name="percent" /> % of the total rounded down, at least 1 Goin, never more
    ///     than the total; a free order has no fee.
    /// </summary>
    /// <param name="total">The order total.</param>
    /// <param name="percent">The fee rate in whole percent.</param>
    /// <returns>The fee in Goins.</returns>
    public static long ComputeFee(long total, int percent)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(total);
        ArgumentOutOfRangeException.ThrowIfNegative(percent);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(percent, 100);
        if (total == 0)
        {
            return 0;
        }

        return Math.Min(total, Math.Max(1, checked(total * percent) / 100));
    }

    /// <summary>
    ///     Places a new order (the caller pays <see cref="Total" /> into escrow in the same step).
    /// </summary>
    /// <param name="customerProfileId">The ordering customer profile.</param>
    /// <param name="playerProfileId">The player profile providing the service.</param>
    /// <param name="serviceId">The service.</param>
    /// <param name="serviceName">The service name.</param>
    /// <param name="priceUnit">The price unit, if any.</param>
    /// <param name="unitPrice">The price of one unit.</param>
    /// <param name="quantity">How many units, 1 to <see cref="MaxQuantity" />.</param>
    /// <param name="feePercent">The platform fee in percent.</param>
    /// <param name="now">The current time.</param>
    /// <param name="acceptWindow">How long the player has to accept.</param>
    /// <returns>The pending order.</returns>
    public static Order Place(
        Guid customerProfileId,
        Guid playerProfileId,
        Guid serviceId,
        string serviceName,
        string? priceUnit,
        long unitPrice,
        int quantity,
        int feePercent,
        DateTimeOffset now,
        TimeSpan acceptWindow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);
        ArgumentOutOfRangeException.ThrowIfNegative(unitPrice);
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(quantity, MaxQuantity);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(acceptWindow, TimeSpan.Zero);
        if (customerProfileId == playerProfileId)
        {
            throw new ArgumentException("A profile cannot order from itself.", nameof(playerProfileId));
        }

        long total = checked(unitPrice * quantity);
        return new Order
        {
            Id = Guid.CreateVersion7(),
            CustomerProfileId = customerProfileId,
            PlayerProfileId = playerProfileId,
            ServiceId = serviceId,
            ServiceName = serviceName,
            PriceUnit = priceUnit,
            UnitPrice = unitPrice,
            Quantity = quantity,
            Total = total,
            Fee = ComputeFee(total, feePercent),
            Status = OrderStatus.Pending,
            CreatedAt = now,
            AcceptDeadline = now + acceptWindow,
        };
    }

    /// <summary>
    ///     Recreates an order from storage.
    /// </summary>
    /// <param name="snapshot">The stored values.</param>
    /// <returns>The order.</returns>
    public static Order Restore(OrderSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return new Order
        {
            Id = snapshot.Id,
            CustomerProfileId = snapshot.CustomerProfileId,
            PlayerProfileId = snapshot.PlayerProfileId,
            ServiceId = snapshot.ServiceId,
            ServiceName = snapshot.ServiceName,
            PriceUnit = snapshot.PriceUnit,
            UnitPrice = snapshot.UnitPrice,
            Quantity = snapshot.Quantity,
            Total = snapshot.Total,
            Fee = snapshot.Fee,
            Status = snapshot.Status,
            Outcome = snapshot.Outcome,
            Cancellation = snapshot.Cancellation,
            CreatedAt = snapshot.CreatedAt,
            AcceptDeadline = snapshot.AcceptDeadline,
            AcceptedAt = snapshot.AcceptedAt,
            FinishedAt = snapshot.FinishedAt,
            ConfirmDeadline = snapshot.ConfirmDeadline,
            DisputedAt = snapshot.DisputedAt,
            DisputeReason = snapshot.DisputeReason,
            ClosedAt = snapshot.ClosedAt,
            ResolvedByUserId = snapshot.ResolvedByUserId,
        };
    }

    /// <summary>
    ///     Gets the values to store.
    /// </summary>
    /// <returns>The snapshot.</returns>
    public OrderSnapshot ToSnapshot() => new(
        this.Id,
        this.CustomerProfileId,
        this.PlayerProfileId,
        this.ServiceId,
        this.ServiceName,
        this.PriceUnit,
        this.UnitPrice,
        this.Quantity,
        this.Total,
        this.Fee,
        this.Status,
        this.Outcome,
        this.Cancellation,
        this.CreatedAt,
        this.AcceptDeadline,
        this.AcceptedAt,
        this.FinishedAt,
        this.ConfirmDeadline,
        this.DisputedAt,
        this.DisputeReason,
        this.ClosedAt,
        this.ResolvedByUserId);

    /// <summary>The customer cancels before the player accepted.</summary>
    /// <param name="now">The current time.</param>
    /// <returns>The money to move.</returns>
    public MoneyMove CancelByCustomer(DateTimeOffset now) => this.CancelPending(OrderCancellation.CustomerCancelled, now);

    /// <summary>The player declines a new order.</summary>
    /// <param name="now">The current time.</param>
    /// <returns>The money to move.</returns>
    public MoneyMove Decline(DateTimeOffset now) => this.CancelPending(OrderCancellation.PlayerDeclined, now);

    /// <summary>Cancels an order the player did not accept in time.</summary>
    /// <param name="now">The current time; must be at or after <see cref="AcceptDeadline" />.</param>
    /// <returns>The money to move.</returns>
    public MoneyMove ExpireUnaccepted(DateTimeOffset now)
    {
        if (now < this.AcceptDeadline)
        {
            throw new InvalidOperationException($"Order {this.Id} can still be accepted.");
        }

        return this.CancelPending(OrderCancellation.AcceptTimeout, now);
    }

    /// <summary>The player accepts a new order.</summary>
    /// <param name="now">The current time; must be before <see cref="AcceptDeadline" />.</param>
    /// <returns>The money to move.</returns>
    public MoneyMove Accept(DateTimeOffset now)
    {
        this.Require(OrderStatus.Pending);
        if (now >= this.AcceptDeadline)
        {
            throw new InvalidOperationException($"Order {this.Id} can no longer be accepted.");
        }

        this.Status = OrderStatus.Active;
        this.AcceptedAt = now;
        return MoneyMove.None;
    }

    /// <summary>The player ends the order with an outcome; the customer then has a window to confirm or report.</summary>
    /// <param name="outcome">How it ended.</param>
    /// <param name="now">The current time.</param>
    /// <param name="confirmWindow">How long the customer has to confirm or report.</param>
    /// <returns>The money to move.</returns>
    public MoneyMove Finish(OrderOutcome outcome, DateTimeOffset now, TimeSpan confirmWindow)
    {
        this.Require(OrderStatus.Active);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(confirmWindow, TimeSpan.Zero);
        this.Status = OrderStatus.Finished;
        this.Outcome = outcome;
        this.FinishedAt = now;
        this.ConfirmDeadline = now + confirmWindow;
        return MoneyMove.None;
    }

    /// <summary>The customer confirms a finished order; the player is paid.</summary>
    /// <param name="now">The current time.</param>
    /// <returns>The money to move.</returns>
    public MoneyMove Confirm(DateTimeOffset now)
    {
        this.Require(OrderStatus.Finished);
        return this.Close(OrderStatus.Completed, now);
    }

    /// <summary>Completes a finished order the customer neither confirmed nor reported in time.</summary>
    /// <param name="now">The current time; must be at or after <see cref="ConfirmDeadline" />.</param>
    /// <returns>The money to move.</returns>
    public MoneyMove AutoComplete(DateTimeOffset now)
    {
        this.Require(OrderStatus.Finished);
        if (now < this.ConfirmDeadline)
        {
            throw new InvalidOperationException($"Order {this.Id} is still waiting for the customer.");
        }

        return this.Close(OrderStatus.Completed, now);
    }

    /// <summary>The customer reports a problem; the Goins stay in escrow until an admin decides.</summary>
    /// <param name="reason">What went wrong.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The money to move.</returns>
    public MoneyMove Report(string reason, DateTimeOffset now)
    {
        if (this.Status is not (OrderStatus.Active or OrderStatus.Finished))
        {
            throw new InvalidOperationException($"Order {this.Id} cannot be reported in state {this.Status}.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        string trimmed = reason.Trim();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmed.Length, DisputeReasonMaxLength, nameof(reason));
        this.Status = OrderStatus.Disputed;
        this.DisputedAt = now;
        this.DisputeReason = trimmed;
        return MoneyMove.None;
    }

    /// <summary>An admin decides a dispute for the player: the order completes and the player is paid.</summary>
    /// <param name="adminUserId">The admin.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The money to move.</returns>
    public MoneyMove ResolveForPlayer(Guid adminUserId, DateTimeOffset now)
    {
        this.Require(OrderStatus.Disputed);
        this.ResolvedByUserId = adminUserId;

        // Reported while still active: the admin's decision says the service happened.
        this.Outcome ??= OrderOutcome.Delivered;
        return this.Close(OrderStatus.Completed, now);
    }

    /// <summary>An admin decides a dispute for the customer: the order is cancelled and refunded.</summary>
    /// <param name="adminUserId">The admin.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The money to move.</returns>
    public MoneyMove ResolveForCustomer(Guid adminUserId, DateTimeOffset now)
    {
        this.Require(OrderStatus.Disputed);
        this.ResolvedByUserId = adminUserId;
        this.Cancellation = OrderCancellation.AdminRefunded;
        return this.Close(OrderStatus.Cancelled, now);
    }

    private MoneyMove CancelPending(OrderCancellation reason, DateTimeOffset now)
    {
        this.Require(OrderStatus.Pending);
        this.Cancellation = reason;
        return this.Close(OrderStatus.Cancelled, now);
    }

    private MoneyMove Close(OrderStatus status, DateTimeOffset now)
    {
        this.Status = status;
        this.ClosedAt = now;
        return status == OrderStatus.Completed ? MoneyMove.Payout : MoneyMove.Refund;
    }

    private void Require(OrderStatus status)
    {
        if (this.Status != status)
        {
            throw new InvalidOperationException($"Order {this.Id} is {this.Status}, not {status}.");
        }
    }
}

/// <summary>
///     The stored values of an <see cref="Order" />.
/// </summary>
#pragma warning disable SA1600 // Same members as Order, documented there.
public sealed record OrderSnapshot(
    Guid Id,
    Guid CustomerProfileId,
    Guid PlayerProfileId,
    Guid ServiceId,
    string ServiceName,
    string? PriceUnit,
    long UnitPrice,
    int Quantity,
    long Total,
    long Fee,
    OrderStatus Status,
    OrderOutcome? Outcome,
    OrderCancellation? Cancellation,
    DateTimeOffset CreatedAt,
    DateTimeOffset AcceptDeadline,
    DateTimeOffset? AcceptedAt,
    DateTimeOffset? FinishedAt,
    DateTimeOffset? ConfirmDeadline,
    DateTimeOffset? DisputedAt,
    string? DisputeReason,
    DateTimeOffset? ClosedAt,
    Guid? ResolvedByUserId);
#pragma warning restore SA1600
