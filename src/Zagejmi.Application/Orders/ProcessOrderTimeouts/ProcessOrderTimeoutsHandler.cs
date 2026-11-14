using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Zagejmi.Application.Orders.Abstractions;
using Zagejmi.Domain.Orders;

namespace Zagejmi.Application.Orders.ProcessOrderTimeouts;

/// <summary>
///     Applies the order deadlines: cancels and refunds orders the player did not accept in time, and completes (pays
///     out) finished orders the customer neither confirmed nor reported in time. Runs periodically.
/// </summary>
public sealed partial class ProcessOrderTimeoutsHandler
{
    private const int BatchSize = 100;

    private readonly IOrderRepository orderRepository;
    private readonly OrderMoney orderMoney;
    private readonly TimeProvider clock;
    private readonly ILogger<ProcessOrderTimeoutsHandler> logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProcessOrderTimeoutsHandler" /> class.
    /// </summary>
    /// <param name="orderRepository">The order repository.</param>
    /// <param name="orderMoney">Stores each change with its money movement.</param>
    /// <param name="clock">The clock.</param>
    /// <param name="logger">The logger.</param>
    public ProcessOrderTimeoutsHandler(
        IOrderRepository orderRepository,
        OrderMoney orderMoney,
        TimeProvider clock,
        ILogger<ProcessOrderTimeoutsHandler> logger)
    {
        this.orderRepository = orderRepository;
        this.orderMoney = orderMoney;
        this.clock = clock;
        this.logger = logger;
    }

    /// <summary>
    ///     Processes every order whose deadline has passed.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>How many orders were changed.</returns>
    public async Task<int> Handle(CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = this.clock.GetUtcNow();
        int changed = 0;
        while (true)
        {
            IReadOnlyList<Order> due = await this.orderRepository.GetDueAsync(now, BatchSize, cancellationToken);
            int changedInBatch = 0;
            foreach (Order order in due)
            {
                OrderStatus previous = order.Status;
                Order.MoneyMove move = previous == OrderStatus.Pending
                    ? order.ExpireUnaccepted(now)
                    : order.AutoComplete(now);

                OrderWriteOutcome outcome = await this.orderMoney.SaveAsync(order, previous, move, now, cancellationToken);
                if (outcome == OrderWriteOutcome.Saved)
                {
                    changedInBatch++;
                }
                else
                {
                    // Someone acted on it at the same moment (e.g. the player accepted at the deadline); their change wins.
                    this.LogSkipped(order.Id, outcome);
                }
            }

            changed += changedInBatch;

            // A batch where nothing could be changed would come back unchanged; stop instead of looping.
            if (due.Count < BatchSize || changedInBatch == 0)
            {
                return changed;
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Skipped timing out order {OrderId}: {Outcome}.")]
    private partial void LogSkipped(Guid orderId, OrderWriteOutcome outcome);
}
