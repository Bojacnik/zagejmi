using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Application.Orders.Abstractions;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Orders;
using Zagejmi.Shared;

namespace Zagejmi.Application.Orders.ChangeOrder;

/// <summary>
///     Applies a customer's or player's action to an order and moves the money it implies (refund on cancel/decline,
///     payout on confirm). Only the actions in <see cref="OrderActions.Available" /> are allowed.
/// </summary>
public sealed class ChangeOrderHandler : IHandlerRequest<ChangeOrderCommand, Result>
{
    private readonly IUserRepository userRepository;
    private readonly IProfileRepository profileRepository;
    private readonly IOrderRepository orderRepository;
    private readonly OrderMoney orderMoney;
    private readonly TimeProvider clock;
    private readonly OrderOptions options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ChangeOrderHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="profileRepository">The profile repository.</param>
    /// <param name="orderRepository">The order repository.</param>
    /// <param name="orderMoney">Stores the change with its money movement.</param>
    /// <param name="clock">The clock.</param>
    /// <param name="options">The order settings.</param>
    public ChangeOrderHandler(
        IUserRepository userRepository,
        IProfileRepository profileRepository,
        IOrderRepository orderRepository,
        OrderMoney orderMoney,
        TimeProvider clock,
        IOptions<OrderOptions> options)
    {
        this.userRepository = userRepository;
        this.profileRepository = profileRepository;
        this.orderRepository = orderRepository;
        this.orderMoney = orderMoney;
        this.clock = clock;
        this.options = options.Value;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(ChangeOrderCommand request, CancellationToken cancellationToken = default)
    {
        User? user = await this.userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(ErrorDetail.NotFound(nameof(User), request.UserId));
        }

        if (user.IsGhostMode)
        {
            return Result.Failure(OrderErrors.GhostMode);
        }

        Order? order = await this.orderRepository.GetAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound(request.OrderId));
        }

        OrderRole role = OrderActions.RoleOf(request.Action);
        Guid? myProfileId = role == OrderRole.Customer
            ? (await this.profileRepository.GetCustomerByUserIdAsync(user.Id, cancellationToken))?.Id
            : (await this.profileRepository.GetPlayerByUserIdAsync(user.Id, cancellationToken))?.Id;
        Guid sideProfileId = role == OrderRole.Customer ? order.CustomerProfileId : order.PlayerProfileId;
        if (myProfileId != sideProfileId)
        {
            return Result.Failure(OrderErrors.NotYours);
        }

        DateTimeOffset now = this.clock.GetUtcNow();
        if (!OrderActions.Available(order, role, now).Contains(request.Action))
        {
            return Result.Failure(OrderErrors.Changed);
        }

        OrderStatus previous = order.Status;
        Result<Order.MoneyMove> applied = this.Apply(order, request, now);
        if (applied.IsFailure)
        {
            return Result.Failure(applied.Error!);
        }

        return await this.SaveAsync(order, previous, applied.Value, now, cancellationToken);
    }

    private async Task<Result> SaveAsync(
        Order order,
        OrderStatus previous,
        Order.MoneyMove move,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        OrderWriteOutcome outcome = await this.orderMoney.SaveAsync(order, previous, move, now, cancellationToken);
        return outcome switch
        {
            OrderWriteOutcome.Saved => Result.Success(),
            OrderWriteOutcome.StateChanged => Result.Failure(OrderErrors.Changed),
            _ => throw new InvalidOperationException($"Unexpected outcome {outcome} for order {order.Id}."),
        };
    }

    private Result<Order.MoneyMove> Apply(Order order, ChangeOrderCommand request, DateTimeOffset now)
    {
        switch (request.Action)
        {
            case OrderAction.Cancel:
                return order.CancelByCustomer(now);
            case OrderAction.Accept:
                return order.Accept(now);
            case OrderAction.Decline:
                return order.Decline(now);
            case OrderAction.Confirm:
                return order.Confirm(now);
            case OrderAction.Finish:
                if (request.Outcome is not { } outcome || !Enum.IsDefined(outcome))
                {
                    return Result<Order.MoneyMove>.Failure(ErrorDetail.Validation(
                        "Order.Outcome",
                        "Vyberte, jak objednávka skončila."));
                }

                return order.Finish(outcome, now, this.options.ConfirmWindow);
            case OrderAction.Report:
                string reason = request.Reason?.Trim() ?? string.Empty;
                if (reason.Length is 0 or > Order.DisputeReasonMaxLength)
                {
                    return Result<Order.MoneyMove>.Failure(ErrorDetail.Validation(
                        "Order.DisputeReason",
                        $"Popište problém (nejvýše {Order.DisputeReasonMaxLength} znaků)."));
                }

                return order.Report(reason, now);
            default:
                throw new ArgumentOutOfRangeException(nameof(request), request.Action, "Unknown order action.");
        }
    }
}
