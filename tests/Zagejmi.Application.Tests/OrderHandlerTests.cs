using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Xunit;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Admin;
using Zagejmi.Application.Orders;
using Zagejmi.Application.Orders.ChangeOrder;
using Zagejmi.Application.Orders.GetDisputedOrders;
using Zagejmi.Application.Orders.GetOrders;
using Zagejmi.Application.Orders.PlaceOrder;
using Zagejmi.Application.Orders.ProcessOrderTimeouts;
using Zagejmi.Application.Orders.ResolveDispute;
using Zagejmi.Application.Wallets.GrantReward;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Orders;
using Zagejmi.Domain.Profiles.Customers;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Domain.Wallets;
using Zagejmi.Shared;

namespace Zagejmi.Application.Tests;

public sealed class OrderHandlerTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Order_pays_bonus_first_then_paid_goins_into_escrow()
    {
        Setup s = new();
        await s.GiveBonusAsync(30);
        s.GivePaid(200);

        Result<Guid> placed = await s.Place(s.Service.Id, quantity: 2); // 2 × 50 = 100

        Order order = (await s.Orders.GetAsync(placed.Value, Ct))!;
        Assert.Equal(100, order.Total);
        Assert.Equal(10, order.Fee);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(0, s.CustomerBonusBalance);
        Assert.Equal(130, s.CustomerPaidBalance);
        Assert.Equal(100, s.World.Wallets.Balance(SystemAccounts.EscrowId));
    }

    [Fact]
    public async Task Order_is_refused_without_enough_goins_and_nothing_moves()
    {
        Setup s = new();
        await s.GiveBonusAsync(20);
        s.GivePaid(10);

        Result<Guid> placed = await s.Place(s.Service.Id, quantity: 1);

        Assert.Equal("Order.InsufficientFunds", placed.Error?.Code);
        Assert.Contains("50 Goinů", placed.Error!.Message);
        Assert.Contains("30 Goinů", placed.Error.Message);
        Assert.Empty(s.Orders.Stored);
        Assert.Equal(10, s.CustomerPaidBalance);
    }

    [Fact]
    public async Task Ghosts_own_services_unavailable_services_and_bad_quantities_are_refused()
    {
        Setup s = new();
        s.GivePaid(1000);

        Assert.Equal("Order.Quantity", (await s.Place(s.Service.Id, quantity: 11)).Error?.Code);
        Assert.Equal("Order.OwnService", (await s.PlaceAs(s.PlayerUser, s.Service.Id)).Error?.Code);

        s.Service.IsAvailable = false;
        Assert.Equal("Order.Unavailable", (await s.Place(s.Service.Id)).Error?.Code);
        s.Service.IsAvailable = true;

        s.CustomerUser.IsGhostMode = true;
        Assert.Equal("User.GhostMode", (await s.Place(s.Service.Id)).Error?.Code);
        Assert.Empty(s.Orders.Stored);
    }

    [Fact]
    public async Task Delivered_and_confirmed_order_pays_the_player_in_paid_goins_minus_the_fee()
    {
        Setup s = new();
        await s.GiveBonusAsync(30);
        s.GivePaid(200);
        Guid orderId = (await s.Place(s.Service.Id, quantity: 2)).Value;

        Assert.True((await s.Act(s.PlayerUser, orderId, OrderAction.Accept)).IsSuccess);
        Assert.True((await s.Act(s.PlayerUser, orderId, OrderAction.Finish, OrderOutcome.Delivered)).IsSuccess);
        Assert.True((await s.Act(s.CustomerUser, orderId, OrderAction.Confirm)).IsSuccess);

        Order order = (await s.Orders.GetAsync(orderId, Ct))!;
        Assert.Equal(OrderStatus.Completed, order.Status);
        Assert.True(order.UnlocksReview);
        Assert.Equal(90, s.PlayerBalance);
        Assert.Equal(0, s.World.Wallets.Balance(SystemAccounts.EscrowId));

        // Escrow held 70 paid + 30 bonus; the player's share is 100 − 10 = 90. All 70 paid Goins go to the player and
        // the promo budget adds 20 paid Goins; the 30 bonus Goins return to the promo budget. The paid Goins don't
        // exceed the share, so the platform earns no fee on this order (the promo budget keeps the difference).
        Assert.Equal(0, s.World.Wallets.Balance(SystemAccounts.PlatformRevenueId));
        Assert.Equal(-20, s.World.Wallets.Balance(SystemAccounts.PromoBudgetId)); // −30 granted, then −20 + 30 at payout
        LedgerTransaction payout = s.World.Wallets.Posted.Single(t => t.Type == LedgerTransactionType.OrderPayout);
        Assert.All(payout.Entries.Where(e => e.AccountKind == AccountKind.PlayerEarnings), e => Assert.Equal(Funding.Paid, e.Funding));
    }

    [Fact]
    public async Task Customer_cancels_a_pending_order_and_gets_everything_back()
    {
        Setup s = new();
        BonusGrant grant = await s.GiveBonusAsync(30);
        s.GivePaid(200);
        Guid orderId = (await s.Place(s.Service.Id, quantity: 2)).Value;

        Assert.True((await s.Act(s.CustomerUser, orderId, OrderAction.Cancel)).IsSuccess);

        Order order = (await s.Orders.GetAsync(orderId, Ct))!;
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal(OrderCancellation.CustomerCancelled, order.Cancellation);
        Assert.Equal(200, s.CustomerPaidBalance);
        Assert.Equal(30, s.CustomerBonusBalance);
        Assert.Equal(30, s.World.Wallets.Grants.Single(g => g.Id == grant.Id).Remaining.Amount);
    }

    [Fact]
    public async Task Only_the_right_side_can_act_and_only_when_allowed()
    {
        Setup s = new();
        s.GivePaid(200);
        Guid orderId = (await s.Place(s.Service.Id)).Value;
        (User stranger, _) = s.World.AddUser("cizi", verified: true);

        Assert.Equal("Order.NotYours", (await s.Act(s.CustomerUser, orderId, OrderAction.Accept)).Error?.Code);
        Assert.Equal("Order.NotYours", (await s.Act(stranger, orderId, OrderAction.Cancel)).Error?.Code);
        Assert.Equal("Order.Changed", (await s.Act(s.CustomerUser, orderId, OrderAction.Confirm)).Error?.Code);

        await s.Act(s.PlayerUser, orderId, OrderAction.Accept);
        Assert.Equal("Order.Changed", (await s.Act(s.CustomerUser, orderId, OrderAction.Cancel)).Error?.Code);
        Assert.Equal("Order.Outcome", (await s.Act(s.PlayerUser, orderId, OrderAction.Finish)).Error?.Code);
    }

    [Fact]
    public async Task A_change_made_from_a_stale_state_is_refused()
    {
        Setup s = new();
        s.GivePaid(200);
        Guid orderId = (await s.Place(s.Service.Id)).Value;

        // The customer's cancel and the player's accept race: the stored order is already accepted when the cancel is
        // saved from the old Pending state.
        Order stale = (await s.Orders.GetAsync(orderId, Ct))!;
        await s.Act(s.PlayerUser, orderId, OrderAction.Accept);
        Order.MoneyMove move = stale.CancelByCustomer(s.World.Clock.GetUtcNow());

        Assert.Equal(
            Zagejmi.Application.Orders.Abstractions.OrderWriteOutcome.StateChanged,
            await s.Money.SaveAsync(stale, OrderStatus.Pending, move, s.World.Clock.GetUtcNow(), Ct));
        Assert.Equal(OrderStatus.Active, (await s.Orders.GetAsync(orderId, Ct))!.Status);
        Assert.Equal(150, s.CustomerPaidBalance);
    }

    [Fact]
    public async Task Unaccepted_orders_are_refunded_and_unconfirmed_orders_paid_out_by_the_timeout_job()
    {
        Setup s = new();
        s.GivePaid(200);
        Guid unaccepted = (await s.Place(s.Service.Id)).Value;
        Guid unconfirmed = (await s.Place(s.Service.Id)).Value;
        await s.Act(s.PlayerUser, unconfirmed, OrderAction.Accept);
        await s.Act(s.PlayerUser, unconfirmed, OrderAction.Finish, OrderOutcome.CustomerNoShow);

        s.World.Clock.Advance(TimeSpan.FromHours(47));
        Assert.Equal(0, await s.Timeouts.Handle(Ct));

        s.World.Clock.Advance(TimeSpan.FromHours(25)); // 72 h after finishing, 72 h after ordering
        Assert.Equal(2, await s.Timeouts.Handle(Ct));

        Assert.Equal(OrderCancellation.AcceptTimeout, (await s.Orders.GetAsync(unaccepted, Ct))!.Cancellation);
        Order paid = (await s.Orders.GetAsync(unconfirmed, Ct))!;
        Assert.Equal(OrderStatus.Completed, paid.Status);
        Assert.False(paid.UnlocksReview);
        Assert.Equal(150, s.CustomerPaidBalance);
        Assert.Equal(45, s.PlayerBalance);
    }

    [Fact]
    public async Task Refunded_bonus_whose_grant_expired_meanwhile_stays_usable_for_a_week()
    {
        Setup s = new(bonusLifetime: TimeSpan.FromHours(10));
        BonusGrant grant = await s.GiveBonusAsync(50);
        Guid orderId = (await s.Place(s.Service.Id)).Value;

        s.World.Clock.Advance(TimeSpan.FromHours(20)); // the grant has expired while its Goins sat in escrow
        Assert.True((await s.Act(s.PlayerUser, orderId, OrderAction.Decline)).IsSuccess);

        BonusGrant refunded = s.World.Wallets.Grants.Single(g => g.Id == grant.Id);
        Assert.Equal(50, refunded.Remaining.Amount);
        Assert.Equal(s.World.Clock.GetUtcNow() + TimeSpan.FromDays(7), refunded.ExpiresAt);
    }

    [Fact]
    public async Task Reported_order_waits_for_an_admin_who_can_refund_it()
    {
        Setup s = new();
        s.GivePaid(200);
        Guid orderId = (await s.Place(s.Service.Id)).Value;
        await s.Act(s.PlayerUser, orderId, OrderAction.Accept);
        await s.Act(s.PlayerUser, orderId, OrderAction.Finish, OrderOutcome.CustomerNoShow);

        Assert.Equal("Order.DisputeReason", (await s.Act(s.CustomerUser, orderId, OrderAction.Report, reason: "  ")).Error?.Code);
        Assert.True((await s.Act(s.CustomerUser, orderId, OrderAction.Report, reason: "Byl jsem připojený celou dobu.")).IsSuccess);

        // Frozen: the confirm deadline passes without a payout.
        s.World.Clock.Advance(TimeSpan.FromDays(5));
        Assert.Equal(0, await s.Timeouts.Handle(Ct));

        Assert.Equal("Admin.Required", (await s.Resolve(s.CustomerUser, orderId, forCustomer: true)).Error?.Code);
        Assert.Equal("Admin.Required", (await s.Disputed(s.CustomerUser)).Error?.Code);
        Assert.Single((await s.Disputed(s.Admin)).Value);

        Assert.True((await s.Resolve(s.Admin, orderId, forCustomer: true)).IsSuccess);
        Order order = (await s.Orders.GetAsync(orderId, Ct))!;
        Assert.Equal(OrderCancellation.AdminRefunded, order.Cancellation);
        Assert.Equal(s.Admin.Id, order.ResolvedByUserId);
        Assert.Equal(200, s.CustomerPaidBalance);
        Assert.Equal(0, s.PlayerBalance);
        Assert.Empty((await s.Disputed(s.Admin)).Value);
    }

    [Fact]
    public async Task Free_service_goes_through_the_whole_flow_without_moving_goins()
    {
        Setup s = new();
        s.Service.Price = 0;

        Guid orderId = (await s.Place(s.Service.Id, quantity: 3)).Value;
        await s.Act(s.PlayerUser, orderId, OrderAction.Accept);
        await s.Act(s.PlayerUser, orderId, OrderAction.Finish, OrderOutcome.Delivered);
        Assert.True((await s.Act(s.CustomerUser, orderId, OrderAction.Confirm)).IsSuccess);

        Assert.True((await s.Orders.GetAsync(orderId, Ct))!.UnlocksReview);
        Assert.DoesNotContain(s.World.Wallets.Posted, t => t.Type != LedgerTransactionType.Reward);
    }

    [Fact]
    public async Task Each_side_sees_its_orders_with_the_actions_it_may_take()
    {
        Setup s = new();
        s.GivePaid(200);
        await s.Place(s.Service.Id);

        PagedResult<OrderView> mine = (await s.List(s.CustomerUser, OrderRole.Customer)).Value;
        PagedResult<OrderView> incoming = (await s.List(s.PlayerUser, OrderRole.Player)).Value;
        PagedResult<OrderView> playerAsCustomer = (await s.List(s.PlayerUser, OrderRole.Customer)).Value;

        OrderView view = Assert.Single(mine.Items);
        Assert.Equal("jana", view.Customer.DisplayName);
        Assert.Equal("Hráčka Eva", view.Player.DisplayName);
        Assert.Equal([OrderAction.Cancel], view.Actions);
        Assert.Equal([OrderAction.Accept, OrderAction.Decline], Assert.Single(incoming.Items).Actions);
        Assert.Empty(playerAsCustomer.Items);
    }

    /// <summary>
    ///     A customer "jana", a player "Hráčka Eva" with a 50-Goin service, an admin, and all order handlers.
    /// </summary>
    private sealed class Setup
    {
        public Setup(TimeSpan? bonusLifetime = null)
        {
            this.World = new TestWorld();
            if (bonusLifetime is { } lifetime)
            {
                this.World.WalletOptions.Value.BonusLifetime = lifetime;
            }

            (this.CustomerUser, this.Customer) = this.World.AddUser("jana", verified: true);
            (this.PlayerUser, _) = this.World.AddUser("eva", verified: true);
            (this.Admin, _) = this.World.AddUser("admin", verified: true);
            this.Player = new PlayerProfile(Guid.CreateVersion7(), this.PlayerUser.Id, "Hráčka Eva");
            this.World.Profiles.Players.Add(this.Player);
            this.Service = new PlayerService
            {
                Id = Guid.CreateVersion7(),
                PlayerProfileId = this.Player.Id,
                Name = "Valorant duo",
                PriceUnit = "/ hodina",
                Price = 50,
            };
            this.World.Profiles.Services.Add(this.Service);

            this.Orders = new FakeOrderRepository(this.World.Wallets);
            IOptions<OrderOptions> options = Options.Create(new OrderOptions());
            IOptions<AdminOptions> admins = Options.Create(new AdminOptions { Emails = ["ADMIN@example.test"] });
            this.Money = new OrderMoney(this.Orders, this.World.Wallets, options);
            this.PlaceHandler = new PlaceOrderHandler(
                this.World.Users, this.World.Profiles, this.World.Wallets, this.Orders, this.Money, this.World.Clock, options);
            this.Change = new ChangeOrderHandler(
                this.World.Users, this.World.Profiles, this.Orders, this.Money, this.World.Clock, options);
            this.ResolveHandler = new ResolveDisputeHandler(this.World.Users, this.Orders, this.Money, this.World.Clock, admins);
            this.DisputedHandler = new GetDisputedOrdersHandler(this.World.Users, this.World.Profiles, this.Orders, admins);
            this.Timeouts = new ProcessOrderTimeoutsHandler(
                this.Orders, this.Money, this.World.Clock, NullLogger<ProcessOrderTimeoutsHandler>.Instance);
            this.ListHandler = new GetOrdersHandler(this.World.Profiles, this.Orders, this.World.Clock);
        }

        public TestWorld World { get; }

        public User CustomerUser { get; }

        public CustomerProfile Customer { get; }

        public User PlayerUser { get; }

        public User Admin { get; }

        public PlayerProfile Player { get; }

        public PlayerService Service { get; }

        public FakeOrderRepository Orders { get; }

        public OrderMoney Money { get; }

        public PlaceOrderHandler PlaceHandler { get; }

        public ChangeOrderHandler Change { get; }

        public ResolveDisputeHandler ResolveHandler { get; }

        public GetDisputedOrdersHandler DisputedHandler { get; }

        public ProcessOrderTimeoutsHandler Timeouts { get; }

        public GetOrdersHandler ListHandler { get; }

        public long CustomerPaidBalance => this.Balance(this.Customer.Id, AccountKind.CustomerPaid);

        public long CustomerBonusBalance => this.Balance(this.Customer.Id, AccountKind.CustomerBonus);

        public long PlayerBalance => this.Balance(this.Player.Id, AccountKind.PlayerEarnings);

        public async Task<BonusGrant> GiveBonusAsync(long amount)
        {
            await this.World.GrantReward.Handle(
                new GrantRewardCommand
                {
                    CustomerProfileId = this.Customer.Id,
                    Amount = amount,
                    Reason = "Testovací bonus",
                    IdempotencyKey = $"test:{Guid.NewGuid()}",
                },
                Ct);
            return this.World.Wallets.Grants.Last();
        }

        public void GivePaid(long amount)
        {
            Account paid = this.World.Wallets.GetOrOpenAccountAsync(this.Customer.Id, AccountKind.CustomerPaid, Ct).Result;
            paid.Apply(new LedgerEntry(paid.Id, AccountKind.CustomerPaid, amount, Funding.Paid, null));
        }

        public Task<Result<Guid>> Place(Guid serviceId, int quantity = 1) => this.PlaceAs(this.CustomerUser, serviceId, quantity);

        public Task<Result<Guid>> PlaceAs(User user, Guid serviceId, int quantity = 1) =>
            this.PlaceHandler.Handle(new PlaceOrderCommand(user.Id, serviceId, quantity), Ct);

        public Task<Result> Act(User user, Guid orderId, OrderAction action, OrderOutcome? outcome = null, string? reason = null) =>
            this.Change.Handle(new ChangeOrderCommand(user.Id, orderId, action, outcome, reason), Ct);

        public Task<Result> Resolve(User user, Guid orderId, bool forCustomer) =>
            this.ResolveHandler.Handle(new ResolveDisputeCommand(user.Id, orderId, forCustomer), Ct);

        public Task<Result<System.Collections.Generic.IReadOnlyList<OrderView>>> Disputed(User user) =>
            this.DisputedHandler.Handle(user.Id, Ct);

        public Task<Result<PagedResult<OrderView>>> List(User user, OrderRole role) =>
            this.ListHandler.Handle(new GetOrdersQuery(user.Id, role), Ct);

        private long Balance(Guid profileId, AccountKind kind) =>
            this.World.Wallets.Accounts.SingleOrDefault(a => a.OwnerProfileId == profileId && a.Kind == kind)?.Balance ?? 0;
    }
}
