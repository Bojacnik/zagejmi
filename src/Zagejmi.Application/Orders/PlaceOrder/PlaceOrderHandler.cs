using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Application.Orders.Abstractions;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Application.Wallets.Abstractions;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Orders;
using Zagejmi.Domain.Profiles.Customers;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Domain.Wallets;
using Zagejmi.Shared;

namespace Zagejmi.Application.Orders.PlaceOrder;

/// <summary>
///     Places an order: checks the service can be ordered, then stores the order and pays its total into escrow (bonus
///     Goins first) in one step.
/// </summary>
public sealed class PlaceOrderHandler : IHandlerRequest<PlaceOrderCommand, Result<Guid>>
{
    private readonly IUserRepository userRepository;
    private readonly IProfileRepository profileRepository;
    private readonly IWalletRepository walletRepository;
    private readonly IOrderRepository orderRepository;
    private readonly OrderMoney orderMoney;
    private readonly TimeProvider clock;
    private readonly OrderOptions options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PlaceOrderHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="profileRepository">The profile repository.</param>
    /// <param name="walletRepository">The wallet repository.</param>
    /// <param name="orderRepository">The order repository.</param>
    /// <param name="orderMoney">Opens the customer's wallet.</param>
    /// <param name="clock">The clock.</param>
    /// <param name="options">The order settings.</param>
    public PlaceOrderHandler(
        IUserRepository userRepository,
        IProfileRepository profileRepository,
        IWalletRepository walletRepository,
        IOrderRepository orderRepository,
        OrderMoney orderMoney,
        TimeProvider clock,
        IOptions<OrderOptions> options)
    {
        this.userRepository = userRepository;
        this.profileRepository = profileRepository;
        this.walletRepository = walletRepository;
        this.orderRepository = orderRepository;
        this.orderMoney = orderMoney;
        this.clock = clock;
        this.options = options.Value;
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(PlaceOrderCommand request, CancellationToken cancellationToken = default)
    {
        if (request.Quantity is < 1 or > Order.MaxQuantity)
        {
            return Result<Guid>.Failure(ErrorDetail.Validation(
                "Order.Quantity",
                $"Objednat můžete 1 až {Order.MaxQuantity} kusů."));
        }

        User? user = await this.userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result<Guid>.Failure(ErrorDetail.NotFound(nameof(User), request.UserId));
        }

        if (user.IsGhostMode)
        {
            return Result<Guid>.Failure(OrderErrors.GhostMode);
        }

        CustomerProfile? customer = await this.profileRepository.GetCustomerByUserIdAsync(user.Id, cancellationToken);
        PlayerService? service = await this.profileRepository.GetPlayerServiceAsync(request.ServiceId, cancellationToken);
        PlayerProfile? player = service is null
            ? null
            : await this.profileRepository.GetPlayerByIdAsync(service.PlayerProfileId, cancellationToken);
        if (customer is null || service is null || player is null)
        {
            return Result<Guid>.Failure(ErrorDetail.NotFound(nameof(PlayerService), request.ServiceId));
        }

        if (player.UserId == user.Id)
        {
            return Result<Guid>.Failure(new ErrorDetail(
                "Order.OwnService",
                "Vlastní službu si objednat nemůžete.",
                ErrorType.Forbidden));
        }

        if (!service.IsAvailable)
        {
            return Result<Guid>.Failure(ErrorDetail.Validation(
                "Order.Unavailable",
                "Tuto službu teď nejde objednat."));
        }

        DateTimeOffset now = this.clock.GetUtcNow();
        Order order = Order.Place(
            customer.Id,
            player.Id,
            service.Id,
            service.Name,
            service.PriceUnit,
            service.Price,
            request.Quantity,
            this.options.FeePercent,
            now,
            this.options.AcceptWindow);

        LedgerTransaction? payment = null;
        long available = 0;
        if (order.Total > 0)
        {
            (Account paid, Account bonus) = await this.orderMoney.CustomerWalletAsync(customer.Id, cancellationToken);
            IReadOnlyList<BonusGrant> grants =
                await this.walletRepository.GetActiveGrantsAsync(customer.Id, now, cancellationToken);
            EscrowFunding funding = EscrowFunding.Split(order.Total, grants);
            available = paid.Balance + funding.BonusTotal;
            if (funding.Paid > paid.Balance)
            {
                return Result<Guid>.Failure(OrderErrors.InsufficientFunds(order.Total, available));
            }

            payment = LedgerTransaction.OrderPayment(order.Id, order.ServiceName, funding, paid, bonus, now);
        }

        OrderWriteOutcome outcome = await this.orderRepository.PlaceAsync(order, payment, cancellationToken);
        return outcome switch
        {
            OrderWriteOutcome.Saved => order.Id,

            // The wallet changed between reading it and paying (e.g. two orders at once).
            OrderWriteOutcome.InsufficientFunds => Result<Guid>.Failure(OrderErrors.InsufficientFunds(order.Total, available)),
            _ => throw new InvalidOperationException($"Unexpected outcome {outcome} when placing order {order.Id}."),
        };
    }
}
