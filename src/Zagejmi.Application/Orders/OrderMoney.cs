using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Zagejmi.Application.Orders.Abstractions;
using Zagejmi.Application.Wallets.Abstractions;
using Zagejmi.Domain.Orders;
using Zagejmi.Domain.Wallets;

namespace Zagejmi.Application.Orders;

/// <summary>
///     Turns an order transition into the ledger transaction it needs and stores both together.
/// </summary>
public sealed class OrderMoney
{
    private readonly IOrderRepository orderRepository;
    private readonly IWalletRepository walletRepository;
    private readonly OrderOptions options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OrderMoney" /> class.
    /// </summary>
    /// <param name="orderRepository">The order repository.</param>
    /// <param name="walletRepository">The wallet repository.</param>
    /// <param name="options">The order settings.</param>
    public OrderMoney(IOrderRepository orderRepository, IWalletRepository walletRepository, IOptions<OrderOptions> options)
    {
        this.orderRepository = orderRepository;
        this.walletRepository = walletRepository;
        this.options = options.Value;
    }

    /// <summary>
    ///     Stores a transition made on <paramref name="order" /> (from <paramref name="previousStatus" />) with its payout
    ///     or refund.
    /// </summary>
    /// <param name="order">The order after the transition.</param>
    /// <param name="previousStatus">The state before the transition.</param>
    /// <param name="move">What the transition does with the escrow.</param>
    /// <param name="now">The current time.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Whether it was stored.</returns>
    public async Task<OrderWriteOutcome> SaveAsync(
        Order order,
        OrderStatus previousStatus,
        Order.MoneyMove move,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        LedgerTransaction? money = null;
        DateTimeOffset? bonusUsableUntil = null;

        if (move != Order.MoneyMove.None && order.Total > 0)
        {
            EscrowFunding funding = await this.orderRepository.GetEscrowFundingAsync(order.Id, cancellationToken);
            if (funding.Total != order.Total)
            {
                throw new InvalidOperationException(
                    $"Escrow of order {order.Id} holds {funding.Total} Goins instead of {order.Total}.");
            }

            if (move == Order.MoneyMove.Payout)
            {
                Account earnings = await this.walletRepository.GetOrOpenAccountAsync(
                    order.PlayerProfileId,
                    AccountKind.PlayerEarnings,
                    cancellationToken);
                money = LedgerTransaction.OrderPayout(order.Id, order.ServiceName, funding, order.Fee, earnings, now);
            }
            else
            {
                (Account paid, Account bonus) = await this.CustomerWalletAsync(order.CustomerProfileId, cancellationToken);
                money = LedgerTransaction.OrderRefund(order.Id, order.ServiceName, funding, paid, bonus, now);
                bonusUsableUntil = now + this.options.RefundedBonusGrace;
            }
        }

        return await this.orderRepository.SaveTransitionAsync(
            order,
            previousStatus,
            money,
            bonusUsableUntil,
            cancellationToken);
    }

    /// <summary>
    ///     Gets (or opens) a customer's paid and bonus accounts.
    /// </summary>
    /// <param name="customerProfileId">The customer profile.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The two accounts.</returns>
    public async Task<(Account Paid, Account Bonus)> CustomerWalletAsync(
        Guid customerProfileId,
        CancellationToken cancellationToken)
    {
        Account paid = await this.walletRepository.GetOrOpenAccountAsync(
            customerProfileId,
            AccountKind.CustomerPaid,
            cancellationToken);
        Account bonus = await this.walletRepository.GetOrOpenAccountAsync(
            customerProfileId,
            AccountKind.CustomerBonus,
            cancellationToken);
        return (paid, bonus);
    }
}
