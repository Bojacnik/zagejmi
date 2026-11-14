using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Admin;
using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Application.Orders.Abstractions;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Orders;
using Zagejmi.Shared;

namespace Zagejmi.Application.Orders.ResolveDispute;

/// <summary>
///     An admin decides a reported order.
/// </summary>
/// <param name="AdminUserId">The signed-in admin.</param>
/// <param name="OrderId">The disputed order.</param>
/// <param name="ForCustomer"><see langword="true" /> refunds the customer, <see langword="false" /> pays the player.</param>
public sealed record ResolveDisputeCommand(Guid AdminUserId, Guid OrderId, bool ForCustomer);

/// <summary>
///     Resolves a dispute: refund the customer or pay the player.
/// </summary>
public sealed class ResolveDisputeHandler : IHandlerRequest<ResolveDisputeCommand, Result>
{
    private readonly IUserRepository userRepository;
    private readonly IOrderRepository orderRepository;
    private readonly OrderMoney orderMoney;
    private readonly TimeProvider clock;
    private readonly AdminOptions admins;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ResolveDisputeHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="orderRepository">The order repository.</param>
    /// <param name="orderMoney">Stores the decision with its money movement.</param>
    /// <param name="clock">The clock.</param>
    /// <param name="admins">Who the admins are.</param>
    public ResolveDisputeHandler(
        IUserRepository userRepository,
        IOrderRepository orderRepository,
        OrderMoney orderMoney,
        TimeProvider clock,
        IOptions<AdminOptions> admins)
    {
        this.userRepository = userRepository;
        this.orderRepository = orderRepository;
        this.orderMoney = orderMoney;
        this.clock = clock;
        this.admins = admins.Value;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(ResolveDisputeCommand request, CancellationToken cancellationToken = default)
    {
        User? admin = await this.userRepository.GetByIdAsync(request.AdminUserId, cancellationToken);
        if (admin is null || !this.admins.IsAdmin(admin))
        {
            return Result.Failure(new ErrorDetail("Admin.Required", "Tohle může jen administrátor.", ErrorType.Forbidden));
        }

        Order? order = await this.orderRepository.GetAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound(request.OrderId));
        }

        if (order.Status != OrderStatus.Disputed)
        {
            return Result.Failure(OrderErrors.Changed);
        }

        DateTimeOffset now = this.clock.GetUtcNow();
        Order.MoneyMove move = request.ForCustomer
            ? order.ResolveForCustomer(admin.Id, now)
            : order.ResolveForPlayer(admin.Id, now);

        OrderWriteOutcome outcome = await this.orderMoney.SaveAsync(order, OrderStatus.Disputed, move, now, cancellationToken);
        return outcome == OrderWriteOutcome.Saved ? Result.Success() : Result.Failure(OrderErrors.Changed);
    }
}
