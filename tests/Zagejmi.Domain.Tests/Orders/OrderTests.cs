using System;

using Xunit;

using Zagejmi.Domain.Orders;

namespace Zagejmi.Domain.Tests.Orders;

public sealed class OrderTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 14, 12, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan AcceptWindow = TimeSpan.FromHours(48);
    private static readonly TimeSpan ConfirmWindow = TimeSpan.FromHours(72);

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(5, 1)]
    [InlineData(10, 1)]
    [InlineData(19, 1)]
    [InlineData(20, 2)]
    [InlineData(199, 19)]
    [InlineData(1000, 100)]
    public void Fee_is_ten_percent_rounded_down_at_least_one_and_at_most_the_total(long total, long fee)
    {
        Assert.Equal(fee, Order.ComputeFee(total, 10));
    }

    [Fact]
    public void Total_is_unit_price_times_quantity_with_the_fee_fixed_at_order_time()
    {
        Order order = Place(unitPrice: 150, quantity: 3);

        Assert.Equal(450, order.Total);
        Assert.Equal(45, order.Fee);
        Assert.Equal(405, order.PlayerShare);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(Now + AcceptWindow, order.AcceptDeadline);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void Quantity_must_be_one_to_ten(int quantity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Place(quantity: quantity));
    }

    [Fact]
    public void Happy_path_pays_out_on_confirmation_and_unlocks_a_review()
    {
        Order order = Place();

        Assert.Equal(Order.MoneyMove.None, order.Accept(Now.AddHours(1)));
        Assert.Equal(Order.MoneyMove.None, order.Finish(OrderOutcome.Delivered, Now.AddHours(2), ConfirmWindow));
        Assert.Equal(Now.AddHours(2) + ConfirmWindow, order.ConfirmDeadline);
        Assert.False(order.UnlocksReview);

        Assert.Equal(Order.MoneyMove.Payout, order.Confirm(Now.AddHours(3)));
        Assert.Equal(OrderStatus.Completed, order.Status);
        Assert.True(order.UnlocksReview);
    }

    [Fact]
    public void No_show_pays_the_player_but_unlocks_no_review()
    {
        Order order = Place();
        order.Accept(Now);
        order.Finish(OrderOutcome.CustomerNoShow, Now, ConfirmWindow);

        Assert.Equal(Order.MoneyMove.Payout, order.AutoComplete(Now + ConfirmWindow));
        Assert.False(order.UnlocksReview);
    }

    [Fact]
    public void Auto_completion_waits_for_the_confirm_deadline()
    {
        Order order = Place();
        order.Accept(Now);
        order.Finish(OrderOutcome.Delivered, Now, ConfirmWindow);

        Assert.Throws<InvalidOperationException>(() => order.AutoComplete(Now + ConfirmWindow - TimeSpan.FromSeconds(1)));
        Assert.Equal(Order.MoneyMove.Payout, order.AutoComplete(Now + ConfirmWindow));
    }

    [Fact]
    public void Pending_order_can_be_cancelled_declined_or_expire_with_a_refund()
    {
        Order cancelled = Place();
        Assert.Equal(Order.MoneyMove.Refund, cancelled.CancelByCustomer(Now));
        Assert.Equal(OrderCancellation.CustomerCancelled, cancelled.Cancellation);

        Order declined = Place();
        Assert.Equal(Order.MoneyMove.Refund, declined.Decline(Now));
        Assert.Equal(OrderCancellation.PlayerDeclined, declined.Cancellation);

        Order expired = Place();
        Assert.Throws<InvalidOperationException>(() => expired.ExpireUnaccepted(Now + AcceptWindow - TimeSpan.FromSeconds(1)));
        Assert.Equal(Order.MoneyMove.Refund, expired.ExpireUnaccepted(Now + AcceptWindow));
        Assert.Equal(OrderCancellation.AcceptTimeout, expired.Cancellation);
    }

    [Fact]
    public void Accepted_order_can_no_longer_be_cancelled_and_late_acceptance_is_refused()
    {
        Order order = Place();
        order.Accept(Now);
        Assert.Throws<InvalidOperationException>(() => order.CancelByCustomer(Now));

        Order late = Place();
        Assert.Throws<InvalidOperationException>(() => late.Accept(Now + AcceptWindow));
    }

    [Fact]
    public void Report_freezes_the_order_until_an_admin_decides()
    {
        Order forPlayer = Place();
        forPlayer.Accept(Now);
        Assert.Equal(Order.MoneyMove.None, forPlayer.Report("  Hráč se nepřipojil.  ", Now));
        Assert.Equal(OrderStatus.Disputed, forPlayer.Status);
        Assert.Equal("Hráč se nepřipojil.", forPlayer.DisputeReason);
        Assert.Throws<InvalidOperationException>(() => forPlayer.Confirm(Now));

        Guid admin = Guid.CreateVersion7();
        Assert.Equal(Order.MoneyMove.Payout, forPlayer.ResolveForPlayer(admin, Now));
        Assert.Equal(OrderOutcome.Delivered, forPlayer.Outcome);
        Assert.Equal(admin, forPlayer.ResolvedByUserId);

        Order forCustomer = Place();
        forCustomer.Accept(Now);
        forCustomer.Finish(OrderOutcome.CustomerNoShow, Now, ConfirmWindow);
        forCustomer.Report("Byl jsem tam.", Now);
        Assert.Equal(Order.MoneyMove.Refund, forCustomer.ResolveForCustomer(admin, Now));
        Assert.Equal(OrderCancellation.AdminRefunded, forCustomer.Cancellation);
        Assert.False(forCustomer.UnlocksReview);
    }

    [Fact]
    public void Pending_or_closed_order_cannot_be_reported()
    {
        Order pending = Place();
        Assert.Throws<InvalidOperationException>(() => pending.Report("x", Now));

        Order done = Place();
        done.Accept(Now);
        done.Finish(OrderOutcome.Delivered, Now, ConfirmWindow);
        done.Confirm(Now);
        Assert.Throws<InvalidOperationException>(() => done.Report("x", Now));
    }

    [Fact]
    public void Snapshot_round_trips()
    {
        Order order = Place();
        order.Accept(Now);
        order.Finish(OrderOutcome.Delivered, Now, ConfirmWindow);

        Assert.Equal(order.ToSnapshot(), Order.Restore(order.ToSnapshot()).ToSnapshot());
    }

    private static Order Place(long unitPrice = 100, int quantity = 1) => Order.Place(
        Guid.CreateVersion7(),
        Guid.CreateVersion7(),
        Guid.CreateVersion7(),
        "Valorant duo",
        "/ hodina",
        unitPrice,
        quantity,
        10,
        Now,
        AcceptWindow);
}
