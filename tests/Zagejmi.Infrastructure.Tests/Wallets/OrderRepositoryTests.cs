using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Xunit;

using Zagejmi.Application.Orders.Abstractions;
using Zagejmi.Application.Wallets.Abstractions;
using Zagejmi.Domain.Orders;
using Zagejmi.Domain.Wallets;
using Zagejmi.Infrastructure.Wallets.Ctx;
using Zagejmi.Infrastructure.Wallets.Repositories;

namespace Zagejmi.Infrastructure.Tests.Wallets;

/// <summary>
///     Runs the order storage against a real PostgreSQL (set ZAGEJMI_TEST_POSTGRES, see <see cref="WalletRepositoryTests" />):
///     order changes and their money commit together, and a change from a stale state is refused.
/// </summary>
public sealed class OrderRepositoryTests : IAsyncLifetime
{
    private const string ConnectionVariable = "ZAGEJMI_TEST_POSTGRES";

    private static readonly DateTimeOffset Now = new(2026, 11, 14, 12, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan AcceptWindow = TimeSpan.FromHours(48);
    private static readonly TimeSpan ConfirmWindow = TimeSpan.FromHours(72);

    private readonly string? serverConnection = Environment.GetEnvironmentVariable(ConnectionVariable);
    private readonly string databaseName = $"zagejmi_test_{Guid.NewGuid():N}";
    private TestContextFactory? factory;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        if (this.serverConnection is null)
        {
            return;
        }

        string connection = new NpgsqlConnectionStringBuilder(this.serverConnection) { Database = this.databaseName }
            .ConnectionString;
        this.factory = new TestContextFactory(connection);
        await using WalletsContext context = this.factory.CreateDbContext();
        await context.Database.MigrateAsync(Ct);
    }

    public async ValueTask DisposeAsync()
    {
        if (this.factory is null)
        {
            return;
        }

        await using WalletsContext context = this.factory.CreateDbContext();
        await context.Database.EnsureDeletedAsync(Ct);
    }

    [Fact]
    public async Task Placing_and_refunding_moves_bonus_goins_through_escrow_and_back()
    {
        (WalletRepository wallets, OrderRepository orders) = this.CreateRepositories();
        Customer customer = await CustomerWithBonusAsync(wallets, 80);
        Order order = NewOrder(customer.ProfileId, unitPrice: 30, quantity: 2);

        Assert.Equal(OrderWriteOutcome.Saved, await orders.PlaceAsync(order, Payment(order, customer, 60), Ct));
        Assert.Equal(20, await BalanceAsync(wallets, customer.ProfileId, AccountKind.CustomerBonus));

        EscrowFunding funding = await orders.GetEscrowFundingAsync(order.Id, Ct);
        Assert.Equal(0, funding.Paid);
        Assert.Equal([new BonusPart(customer.Grant.Id, 60)], funding.Bonus);

        Order.MoneyMove move = order.Decline(Now.AddHours(1));
        LedgerTransaction refund = LedgerTransaction.OrderRefund(
            order.Id, order.ServiceName, funding, customer.Paid, customer.Bonus, Now.AddHours(1));
        Assert.Equal(Order.MoneyMove.Refund, move);
        Assert.Equal(
            OrderWriteOutcome.Saved,
            await orders.SaveTransitionAsync(order, OrderStatus.Pending, refund, null, Ct));

        Assert.Equal(OrderStatus.Cancelled, (await orders.GetAsync(order.Id, Ct))!.Status);
        Assert.Equal(80, await BalanceAsync(wallets, customer.ProfileId, AccountKind.CustomerBonus));
        await this.AssertLedgerConsistentAsync();
    }

    [Fact]
    public async Task Change_from_a_stale_state_stores_neither_the_order_nor_the_money()
    {
        (WalletRepository wallets, OrderRepository orders) = this.CreateRepositories();
        Customer customer = await CustomerWithBonusAsync(wallets, 50);
        Order order = NewOrder(customer.ProfileId, unitPrice: 50);
        await orders.PlaceAsync(order, Payment(order, customer, 50), Ct);

        // The player accepts…
        Order accepted = (await orders.GetAsync(order.Id, Ct))!;
        accepted.Accept(Now.AddHours(1));
        Assert.Equal(OrderWriteOutcome.Saved, await orders.SaveTransitionAsync(accepted, OrderStatus.Pending, null, null, Ct));

        // …while the customer's cancel was prepared from the old Pending state.
        order.CancelByCustomer(Now.AddHours(1));
        LedgerTransaction refund = LedgerTransaction.OrderRefund(
            order.Id, order.ServiceName, await orders.GetEscrowFundingAsync(order.Id, Ct), customer.Paid, customer.Bonus, Now);
        Assert.Equal(
            OrderWriteOutcome.StateChanged,
            await orders.SaveTransitionAsync(order, OrderStatus.Pending, refund, null, Ct));

        Assert.Equal(OrderStatus.Active, (await orders.GetAsync(order.Id, Ct))!.Status);
        Assert.Equal(0, await BalanceAsync(wallets, customer.ProfileId, AccountKind.CustomerBonus));
        await this.AssertLedgerConsistentAsync();
    }

    [Fact]
    public async Task Payout_of_a_bonus_paid_order_gives_the_player_paid_goins_from_the_promo_budget()
    {
        (WalletRepository wallets, OrderRepository orders) = this.CreateRepositories();
        Customer customer = await CustomerWithBonusAsync(wallets, 100);
        Guid playerProfileId = Guid.CreateVersion7();
        Order order = NewOrder(customer.ProfileId, unitPrice: 100, playerProfileId: playerProfileId);
        await orders.PlaceAsync(order, Payment(order, customer, 100), Ct);

        order.Accept(Now);
        await orders.SaveTransitionAsync(order, OrderStatus.Pending, null, null, Ct);
        order.Finish(OrderOutcome.Delivered, Now, ConfirmWindow);
        await orders.SaveTransitionAsync(order, OrderStatus.Active, null, null, Ct);
        order.Confirm(Now.AddHours(1));
        Account earnings = await wallets.GetOrOpenAccountAsync(playerProfileId, AccountKind.PlayerEarnings, Ct);
        LedgerTransaction payout = LedgerTransaction.OrderPayout(
            order.Id, order.ServiceName, await orders.GetEscrowFundingAsync(order.Id, Ct), order.Fee, earnings, Now);

        Assert.Equal(OrderWriteOutcome.Saved, await orders.SaveTransitionAsync(order, OrderStatus.Finished, payout, null, Ct));
        Assert.Equal(90, await BalanceAsync(wallets, playerProfileId, AccountKind.PlayerEarnings));
        Assert.True((await orders.GetAsync(order.Id, Ct))!.UnlocksReview);
        await this.AssertLedgerConsistentAsync();
    }

    [Fact]
    public async Task Refund_extends_bonus_grants_that_would_expire_too_soon()
    {
        (WalletRepository wallets, OrderRepository orders) = this.CreateRepositories();
        Customer customer = await CustomerWithBonusAsync(wallets, 40, lifetime: TimeSpan.FromHours(5));
        Order order = NewOrder(customer.ProfileId, unitPrice: 40);
        await orders.PlaceAsync(order, Payment(order, customer, 40), Ct);

        DateTimeOffset later = Now.AddHours(48);
        order.ExpireUnaccepted(later);
        LedgerTransaction refund = LedgerTransaction.OrderRefund(
            order.Id, order.ServiceName, await orders.GetEscrowFundingAsync(order.Id, Ct), customer.Paid, customer.Bonus, later);
        await orders.SaveTransitionAsync(order, OrderStatus.Pending, refund, later.AddDays(7), Ct);

        BonusGrant grant = Assert.Single(await wallets.GetActiveGrantsAsync(customer.ProfileId, later, Ct));
        Assert.Equal(40, grant.Remaining.Amount);
        Assert.Equal(later.AddDays(7), grant.ExpiresAt);
    }

    [Fact]
    public async Task Finds_due_and_disputed_orders_and_lists_each_side_newest_first()
    {
        (_, OrderRepository orders) = this.CreateRepositories();
        Guid customerId = Guid.CreateVersion7();
        Guid playerId = Guid.CreateVersion7();

        Order pending = NewOrder(customerId, 0, playerProfileId: playerId, at: Now);
        Order finished = NewOrder(customerId, 0, playerProfileId: playerId, at: Now.AddMinutes(1));
        Order disputed = NewOrder(customerId, 0, playerProfileId: playerId, at: Now.AddMinutes(2));
        foreach (Order o in new[] { pending, finished, disputed })
        {
            await orders.PlaceAsync(o, null, Ct);
        }

        finished.Accept(Now);
        await orders.SaveTransitionAsync(finished, OrderStatus.Pending, null, null, Ct);
        finished.Finish(OrderOutcome.Delivered, Now, ConfirmWindow);
        await orders.SaveTransitionAsync(finished, OrderStatus.Active, null, null, Ct);
        disputed.Accept(Now);
        await orders.SaveTransitionAsync(disputed, OrderStatus.Pending, null, null, Ct);
        disputed.Report("Nepřipojil se.", Now);
        await orders.SaveTransitionAsync(disputed, OrderStatus.Active, null, null, Ct);

        Assert.Empty(await orders.GetDueAsync(Now.AddHours(47), 10, Ct));
        Assert.Equal([pending.Id], (await orders.GetDueAsync(Now + AcceptWindow, 10, Ct)).Select(o => o.Id));
        Assert.Equal(
            [pending.Id, finished.Id],
            (await orders.GetDueAsync(Now + ConfirmWindow, 10, Ct)).Select(o => o.Id));
        Assert.Equal([disputed.Id], (await orders.GetDisputedAsync(Ct)).Select(o => o.Id));

        Assert.Equal(
            [disputed.Id, finished.Id],
            (await orders.ListForCustomerAsync(customerId, 0, 2, Ct)).Items.Select(o => o.Id));
        Assert.Equal(3, (await orders.ListForPlayerAsync(playerId, 0, 1, Ct)).TotalCount);
    }

    [Fact]
    public async Task Knows_whether_a_customer_has_a_completed_delivered_order_with_a_player()
    {
        (_, OrderRepository orders) = this.CreateRepositories();
        Guid customerId = Guid.CreateVersion7();
        Guid playerId = Guid.CreateVersion7();

        Order noShow = NewOrder(customerId, 0, playerProfileId: playerId);
        await orders.PlaceAsync(noShow, null, Ct);
        noShow.Accept(Now);
        await orders.SaveTransitionAsync(noShow, OrderStatus.Pending, null, null, Ct);
        noShow.Finish(OrderOutcome.CustomerNoShow, Now, ConfirmWindow);
        await orders.SaveTransitionAsync(noShow, OrderStatus.Active, null, null, Ct);
        noShow.Confirm(Now);
        await orders.SaveTransitionAsync(noShow, OrderStatus.Finished, null, null, Ct);
        Assert.False(await orders.HasReviewableOrderAsync(customerId, playerId, Ct));

        Order delivered = NewOrder(customerId, 0, playerProfileId: playerId);
        await orders.PlaceAsync(delivered, null, Ct);
        delivered.Accept(Now);
        await orders.SaveTransitionAsync(delivered, OrderStatus.Pending, null, null, Ct);
        delivered.Finish(OrderOutcome.Delivered, Now, ConfirmWindow);
        await orders.SaveTransitionAsync(delivered, OrderStatus.Active, null, null, Ct);
        Assert.False(await orders.HasReviewableOrderAsync(customerId, playerId, Ct)); // not confirmed yet

        delivered.Confirm(Now);
        await orders.SaveTransitionAsync(delivered, OrderStatus.Finished, null, null, Ct);
        Assert.True(await orders.HasReviewableOrderAsync(customerId, playerId, Ct));
        Assert.False(await orders.HasReviewableOrderAsync(Guid.CreateVersion7(), playerId, Ct));
    }

    private static Order NewOrder(
        Guid customerProfileId,
        long unitPrice,
        int quantity = 1,
        Guid? playerProfileId = null,
        DateTimeOffset? at = null) => Order.Place(
        customerProfileId,
        playerProfileId ?? Guid.CreateVersion7(),
        Guid.CreateVersion7(),
        "Valorant duo",
        "/ hodina",
        unitPrice,
        quantity,
        10,
        at ?? Now,
        AcceptWindow);

    private static LedgerTransaction Payment(Order order, Customer customer, long bonus) => LedgerTransaction.OrderPayment(
        order.Id,
        order.ServiceName,
        new EscrowFunding(0, [new BonusPart(customer.Grant.Id, bonus)]),
        customer.Paid,
        customer.Bonus,
        Now);

    private static async Task<long> BalanceAsync(WalletRepository wallets, Guid profileId, AccountKind kind) =>
        (await wallets.GetAccountsAsync(profileId, Ct)).SingleOrDefault(a => a.Kind == kind)?.Balance ?? 0;

    private static async Task<Customer> CustomerWithBonusAsync(WalletRepository wallets, long amount, TimeSpan? lifetime = null)
    {
        Guid profileId = Guid.CreateVersion7();
        Account paid = await wallets.GetOrOpenAccountAsync(profileId, AccountKind.CustomerPaid, Ct);
        Account bonus = await wallets.GetOrOpenAccountAsync(profileId, AccountKind.CustomerBonus, Ct);
        BonusGrant grant = BonusGrant.Create(profileId, new Goin(amount), "Bonus", Now, lifetime ?? BonusGrant.DefaultLifetime);
        Assert.Equal(
            PostOutcome.Posted,
            await wallets.PostAsync(LedgerTransaction.Reward(grant, bonus, $"test:{grant.Id}"), grant, Ct));
        return new Customer(profileId, paid, bonus, grant);
    }

    private (WalletRepository Wallets, OrderRepository Orders) CreateRepositories()
    {
        Assert.SkipWhen(this.factory is null, $"Set {ConnectionVariable} to run the PostgreSQL order tests.");
        return (new WalletRepository(this.factory!, TimeProvider.System), new OrderRepository(this.factory!));
    }

    /// <summary>
    ///     Every stored balance equals the sum of its entries, and every bonus account equals its grants' remaining sum.
    /// </summary>
    private async Task AssertLedgerConsistentAsync()
    {
        await using WalletsContext context = this.factory!.CreateDbContext();
        var balances = await context.Accounts
            .Select(a => new
            {
                a.Kind,
                a.Balance,
                Sum = context.Entries.Where(e => e.AccountId == a.Id).Sum(e => (long?)e.Amount) ?? 0,
                Grants = context.BonusGrants.Where(g => g.CustomerProfileId == a.OwnerProfileId).Sum(g => (long?)g.Remaining) ?? 0,
            })
            .ToListAsync(Ct);

        Assert.All(balances, b => Assert.Equal(b.Sum, b.Balance));
        Assert.All(balances.Where(b => b.Kind == AccountKind.CustomerBonus), b => Assert.Equal(b.Grants, b.Balance));
    }

    private sealed record Customer(Guid ProfileId, Account Paid, Account Bonus, BonusGrant Grant);

    private sealed class TestContextFactory : IDbContextFactory<WalletsContext>
    {
        private readonly DbContextOptions<WalletsContext> options;

        public TestContextFactory(string connection)
        {
            this.options = new DbContextOptionsBuilder<WalletsContext>()
                .UseNpgsql(connection, npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "wallets"))
                .Options;
        }

        public WalletsContext CreateDbContext() => new(this.options);
    }
}
