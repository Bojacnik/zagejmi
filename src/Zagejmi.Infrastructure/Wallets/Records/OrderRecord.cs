using System;

using Zagejmi.Domain.Orders;

namespace Zagejmi.Infrastructure.Wallets.Records;

/// <summary>
///     Database row of an order (wallets.Orders). It lives next to the ledger so an order change and its money move
///     commit in one database transaction. See <see cref="Order" /> for the meaning of each column.
/// </summary>
public sealed class OrderRecord
{
    /// <summary>Gets or sets <see cref="Order.Id" />.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets <see cref="Order.CustomerProfileId" />.</summary>
    public Guid CustomerProfileId { get; set; }

    /// <summary>Gets or sets <see cref="Order.PlayerProfileId" />.</summary>
    public Guid PlayerProfileId { get; set; }

    /// <summary>Gets or sets <see cref="Order.ServiceId" />.</summary>
    public Guid ServiceId { get; set; }

    /// <summary>Gets or sets <see cref="Order.ServiceName" />.</summary>
    public required string ServiceName { get; set; }

    /// <summary>Gets or sets <see cref="Order.PriceUnit" />.</summary>
    public string? PriceUnit { get; set; }

    /// <summary>Gets or sets <see cref="Order.UnitPrice" />.</summary>
    public long UnitPrice { get; set; }

    /// <summary>Gets or sets <see cref="Order.Quantity" />.</summary>
    public int Quantity { get; set; }

    /// <summary>Gets or sets <see cref="Order.Total" />.</summary>
    public long Total { get; set; }

    /// <summary>Gets or sets <see cref="Order.Fee" />.</summary>
    public long Fee { get; set; }

    /// <summary>Gets or sets <see cref="Order.Status" />.</summary>
    public OrderStatus Status { get; set; }

    /// <summary>Gets or sets <see cref="Order.Outcome" />.</summary>
    public OrderOutcome? Outcome { get; set; }

    /// <summary>Gets or sets <see cref="Order.Cancellation" />.</summary>
    public OrderCancellation? Cancellation { get; set; }

    /// <summary>Gets or sets <see cref="Order.CreatedAt" />.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets <see cref="Order.AcceptDeadline" />.</summary>
    public DateTimeOffset AcceptDeadline { get; set; }

    /// <summary>Gets or sets <see cref="Order.AcceptedAt" />.</summary>
    public DateTimeOffset? AcceptedAt { get; set; }

    /// <summary>Gets or sets <see cref="Order.FinishedAt" />.</summary>
    public DateTimeOffset? FinishedAt { get; set; }

    /// <summary>Gets or sets <see cref="Order.ConfirmDeadline" />.</summary>
    public DateTimeOffset? ConfirmDeadline { get; set; }

    /// <summary>Gets or sets <see cref="Order.DisputedAt" />.</summary>
    public DateTimeOffset? DisputedAt { get; set; }

    /// <summary>Gets or sets <see cref="Order.DisputeReason" />.</summary>
    public string? DisputeReason { get; set; }

    /// <summary>Gets or sets <see cref="Order.ClosedAt" />.</summary>
    public DateTimeOffset? ClosedAt { get; set; }

    /// <summary>Gets or sets <see cref="Order.ResolvedByUserId" />.</summary>
    public Guid? ResolvedByUserId { get; set; }
}
