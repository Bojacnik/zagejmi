using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Orders.Abstractions;
using Zagejmi.Domain.Orders;
using Zagejmi.Domain.Wallets;
using Zagejmi.Infrastructure.Wallets.Ctx;
using Zagejmi.Infrastructure.Wallets.Records;

namespace Zagejmi.Infrastructure.Wallets.Repositories;

/// <summary>
///     PostgreSQL implementation of <see cref="IOrderRepository" />. Every write stores the order and posts its ledger
///     transaction in one database transaction.
/// </summary>
public sealed class OrderRepository : IOrderRepository
{
    private readonly IDbContextFactory<WalletsContext> contextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OrderRepository" /> class.
    /// </summary>
    /// <param name="contextFactory">Creates short-lived contexts.</param>
    public OrderRepository(IDbContextFactory<WalletsContext> contextFactory)
    {
        this.contextFactory = contextFactory;
    }

    /// <inheritdoc />
    public async Task<Order?> GetAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        await using WalletsContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);
        OrderRecord? record = await context.Orders.AsNoTracking().SingleOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        return record is null ? null : ToDomain(record);
    }

    /// <inheritdoc />
    public async Task<OrderWriteOutcome> PlaceAsync(
        Order order,
        LedgerTransaction? payment,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);
        await using WalletsContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        context.Orders.Add(ToRecord(order));
        return await CommitWithMoneyAsync(context, transaction, payment, null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<OrderWriteOutcome> SaveTransitionAsync(
        Order order,
        OrderStatus previousStatus,
        LedgerTransaction? money,
        DateTimeOffset? refundedBonusUsableUntil,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);
        await using WalletsContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        // Update every column, but only where the row still has the status the change was made from.
        OrderRecord record = ToRecord(order);
        context.Orders.Attach(record);
        context.Entry(record).State = EntityState.Modified;
        context.Entry(record).Property(r => r.Status).OriginalValue = previousStatus;

        try
        {
            return await CommitWithMoneyAsync(context, transaction, money, refundedBonusUsableUntil, cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return OrderWriteOutcome.StateChanged;
        }
    }

    /// <inheritdoc />
    public async Task<EscrowFunding> GetEscrowFundingAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        await using WalletsContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);
        string key = $"order:{orderId}";
        var parts = await context.Entries
            .AsNoTracking()
            .Where(e => e.AccountKind == AccountKind.Escrow && e.Amount > 0)
            .Join(
                context.Transactions.Where(t => t.Type == LedgerTransactionType.OrderPayment && t.IdempotencyKey == key),
                e => e.TransactionId,
                t => t.Id,
                (e, _) => new { e.Funding, e.BonusGrantId, e.Amount })
            .ToListAsync(cancellationToken);

        return new EscrowFunding(
            parts.Where(p => p.Funding == Funding.Paid).Sum(p => p.Amount),
            [
                .. parts
                    .Where(p => p.Funding == Funding.Bonus)
                    .GroupBy(p => p.BonusGrantId!.Value)
                    .Select(g => new BonusPart(g.Key, g.Sum(p => p.Amount))),
            ]);
    }

    /// <inheritdoc />
    public Task<PagedResult<Order>> ListForCustomerAsync(
        Guid customerProfileId,
        int skip,
        int take,
        CancellationToken cancellationToken = default) =>
        this.ListAsync(o => o.CustomerProfileId == customerProfileId, skip, take, cancellationToken);

    /// <inheritdoc />
    public Task<PagedResult<Order>> ListForPlayerAsync(
        Guid playerProfileId,
        int skip,
        int take,
        CancellationToken cancellationToken = default) =>
        this.ListAsync(o => o.PlayerProfileId == playerProfileId, skip, take, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Order>> GetDueAsync(DateTimeOffset now, int take, CancellationToken cancellationToken = default)
    {
        await using WalletsContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);
        List<OrderRecord> records = await context.Orders
            .AsNoTracking()
            .Where(o => (o.Status == OrderStatus.Pending && o.AcceptDeadline <= now)
                        || (o.Status == OrderStatus.Finished && o.ConfirmDeadline <= now))
            .OrderBy(o => o.Status == OrderStatus.Pending ? o.AcceptDeadline : o.ConfirmDeadline)
            .ThenBy(o => o.Id)
            .Take(take)
            .ToListAsync(cancellationToken);
        return [.. records.Select(ToDomain)];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Order>> GetDisputedAsync(CancellationToken cancellationToken = default)
    {
        await using WalletsContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);
        List<OrderRecord> records = await context.Orders
            .AsNoTracking()
            .Where(o => o.Status == OrderStatus.Disputed)
            .OrderBy(o => o.DisputedAt)
            .ToListAsync(cancellationToken);
        return [.. records.Select(ToDomain)];
    }

    /// <inheritdoc />
    public async Task<bool> HasReviewableOrderAsync(
        Guid customerProfileId,
        Guid playerProfileId,
        CancellationToken cancellationToken = default)
    {
        await using WalletsContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Orders.AnyAsync(
            o => o.CustomerProfileId == customerProfileId
                 && o.PlayerProfileId == playerProfileId
                 && o.Status == OrderStatus.Completed
                 && o.Outcome == OrderOutcome.Delivered,
            cancellationToken);
    }

    private static OrderRecord ToRecord(Order order)
    {
        OrderSnapshot s = order.ToSnapshot();
        return new OrderRecord
        {
            Id = s.Id,
            CustomerProfileId = s.CustomerProfileId,
            PlayerProfileId = s.PlayerProfileId,
            ServiceId = s.ServiceId,
            ServiceName = s.ServiceName,
            PriceUnit = s.PriceUnit,
            UnitPrice = s.UnitPrice,
            Quantity = s.Quantity,
            Total = s.Total,
            Fee = s.Fee,
            Status = s.Status,
            Outcome = s.Outcome,
            Cancellation = s.Cancellation,
            CreatedAt = s.CreatedAt,
            AcceptDeadline = s.AcceptDeadline,
            AcceptedAt = s.AcceptedAt,
            FinishedAt = s.FinishedAt,
            ConfirmDeadline = s.ConfirmDeadline,
            DisputedAt = s.DisputedAt,
            DisputeReason = s.DisputeReason,
            ClosedAt = s.ClosedAt,
            ResolvedByUserId = s.ResolvedByUserId,
        };
    }

    private static Order ToDomain(OrderRecord r) => Order.Restore(new OrderSnapshot(
        r.Id,
        r.CustomerProfileId,
        r.PlayerProfileId,
        r.ServiceId,
        r.ServiceName,
        r.PriceUnit,
        r.UnitPrice,
        r.Quantity,
        r.Total,
        r.Fee,
        r.Status,
        r.Outcome,
        r.Cancellation,
        r.CreatedAt,
        r.AcceptDeadline,
        r.AcceptedAt,
        r.FinishedAt,
        r.ConfirmDeadline,
        r.DisputedAt,
        r.DisputeReason,
        r.ClosedAt,
        r.ResolvedByUserId));

    private static async Task<OrderWriteOutcome> CommitWithMoneyAsync(
        WalletsContext context,
        IDbContextTransaction transaction,
        LedgerTransaction? money,
        DateTimeOffset? refundedBonusUsableUntil,
        CancellationToken cancellationToken)
    {
        if (money is not null)
        {
            context.Transactions.Add(WalletRepository.ToRecord(money));
        }

        // Saves the order (insert, or the status-checked update) and the ledger transaction rows.
        await context.SaveChangesAsync(cancellationToken);

        if (money is not null)
        {
            if (!await WalletRepository.ApplyBalancesAsync(context, money, cancellationToken)
                || !await WalletRepository.ApplyGrantsAsync(context, money, cancellationToken))
            {
                await transaction.RollbackAsync(cancellationToken);
                return OrderWriteOutcome.InsufficientFunds;
            }

            if (refundedBonusUsableUntil is { } usableUntil)
            {
                await ExtendRefundedGrantsAsync(context, money, usableUntil, cancellationToken);
            }
        }

        await transaction.CommitAsync(cancellationToken);
        return OrderWriteOutcome.Saved;
    }

    private static async Task ExtendRefundedGrantsAsync(
        WalletsContext context,
        LedgerTransaction refund,
        DateTimeOffset usableUntil,
        CancellationToken cancellationToken)
    {
        Guid[] grantIds =
        [
            .. refund.Entries
                .Where(e => e.AccountKind == AccountKind.CustomerBonus && e.Amount > 0)
                .Select(e => e.BonusGrantId!.Value)
                .Distinct(),
        ];
        if (grantIds.Length == 0)
        {
            return;
        }

        // A changed expiry also changes the expiry job's idempotency key, so a grant that already expired once can
        // expire again later.
        await context.BonusGrants
            .Where(g => grantIds.Contains(g.Id) && g.ExpiresAt < usableUntil)
            .ExecuteUpdateAsync(set => set.SetProperty(g => g.ExpiresAt, usableUntil), cancellationToken);
    }

    private async Task<PagedResult<Order>> ListAsync(
        System.Linq.Expressions.Expression<Func<OrderRecord, bool>> filter,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        await using WalletsContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);
        IQueryable<OrderRecord> query = context.Orders.AsNoTracking().Where(filter);
        int total = await query.CountAsync(cancellationToken);
        List<OrderRecord> records = await query
            .OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
        return new PagedResult<Order>([.. records.Select(ToDomain)], total);
    }
}
