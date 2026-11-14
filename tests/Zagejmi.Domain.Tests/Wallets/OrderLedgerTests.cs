using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

using Zagejmi.Domain.Wallets;

namespace Zagejmi.Domain.Tests.Wallets;

public sealed class OrderLedgerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 14, 12, 0, 0, TimeSpan.Zero);

    private readonly Guid customerId = Guid.CreateVersion7();
    private readonly Account customerPaid;
    private readonly Account customerBonus;
    private readonly Account playerEarnings = Account.OpenForProfile(AccountKind.PlayerEarnings, Guid.CreateVersion7());

    public OrderLedgerTests()
    {
        this.customerPaid = Account.OpenForProfile(AccountKind.CustomerPaid, this.customerId);
        this.customerBonus = Account.OpenForProfile(AccountKind.CustomerBonus, this.customerId);
    }

    [Fact]
    public void Split_takes_bonus_from_the_soonest_expiring_grants_first()
    {
        BonusGrant later = Grant(30, Now.AddDays(60));
        BonusGrant sooner = Grant(20, Now.AddDays(10));

        EscrowFunding all = EscrowFunding.Split(100, [later, sooner]);
        Assert.Equal(50, all.Paid);
        Assert.Equal([new BonusPart(sooner.Id, 20), new BonusPart(later.Id, 30)], all.Bonus);

        EscrowFunding some = EscrowFunding.Split(25, [later, sooner]);
        Assert.Equal(0, some.Paid);
        Assert.Equal([new BonusPart(sooner.Id, 20), new BonusPart(later.Id, 5)], some.Bonus);
    }

    [Fact]
    public void Payment_moves_paid_and_bonus_goins_into_escrow()
    {
        BonusGrant grant = Grant(30, Now.AddDays(10));
        EscrowFunding funding = new(70, [new BonusPart(grant.Id, 30)]);

        LedgerTransaction payment = LedgerTransaction.OrderPayment(
            Guid.CreateVersion7(), "Duo", funding, this.customerPaid, this.customerBonus, Now);

        Assert.Equal(LedgerTransactionType.OrderPayment, payment.Type);
        Assert.Equal(-70, Sum(payment, AccountKind.CustomerPaid));
        Assert.Equal(-30, Sum(payment, AccountKind.CustomerBonus));
        Assert.Equal(100, Sum(payment, AccountKind.Escrow));
    }

    [Fact]
    public void Paid_order_pays_the_player_total_minus_fee_and_the_fee_to_the_platform()
    {
        LedgerTransaction payout = LedgerTransaction.OrderPayout(
            Guid.CreateVersion7(), "Duo", new EscrowFunding(100, []), 10, this.playerEarnings, Now);

        Assert.Equal(-100, Sum(payout, AccountKind.Escrow));
        Assert.Equal(90, Sum(payout, AccountKind.PlayerEarnings));
        Assert.Equal(10, Sum(payout, AccountKind.PlatformRevenue));
        Assert.Equal(0, Sum(payout, AccountKind.PromoBudget));
        Assert.All(Entries(payout, AccountKind.PlayerEarnings), e => Assert.Equal(Funding.Paid, e.Funding));
    }

    [Fact]
    public void Bonus_paid_order_still_pays_the_player_in_paid_goins_from_the_promo_budget()
    {
        BonusGrant grant = Grant(60, Now.AddDays(10));
        EscrowFunding funding = new(40, [new BonusPart(grant.Id, 60)]);

        LedgerTransaction payout = LedgerTransaction.OrderPayout(
            Guid.CreateVersion7(), "Duo", funding, 10, this.playerEarnings, Now);

        // The player gets 90 cashable Goins: 40 paid from escrow + 50 paid from the promo budget. The fee (10) is not
        // covered by paid Goins in escrow beyond the share, so the platform earns nothing from this order; the 60 bonus
        // Goins go back to the promo budget.
        Assert.Equal(90, Sum(payout, AccountKind.PlayerEarnings));
        Assert.All(Entries(payout, AccountKind.PlayerEarnings), e => Assert.Equal(Funding.Paid, e.Funding));
        Assert.Equal(0, Sum(payout, AccountKind.PlatformRevenue));
        Assert.Equal(-100, Sum(payout, AccountKind.Escrow));
        Assert.Equal(-50 + 60, Sum(payout, AccountKind.PromoBudget));
        Assert.Equal(-60, Entries(payout, AccountKind.Escrow).Where(e => e.Funding == Funding.Bonus).Sum(e => e.Amount));
    }

    [Fact]
    public void Mostly_paid_order_takes_the_fee_from_the_paid_part()
    {
        BonusGrant grant = Grant(5, Now.AddDays(10));
        EscrowFunding funding = new(95, [new BonusPart(grant.Id, 5)]);

        LedgerTransaction payout = LedgerTransaction.OrderPayout(
            Guid.CreateVersion7(), "Duo", funding, 10, this.playerEarnings, Now);

        Assert.Equal(90, Sum(payout, AccountKind.PlayerEarnings));
        Assert.Equal(5, Sum(payout, AccountKind.PlatformRevenue));
        Assert.Equal(5, Sum(payout, AccountKind.PromoBudget));
    }

    [Fact]
    public void Refund_returns_each_part_where_it_came_from()
    {
        BonusGrant first = Grant(10, Now.AddDays(5));
        BonusGrant second = Grant(20, Now.AddDays(9));
        EscrowFunding funding = new(70, [new BonusPart(first.Id, 10), new BonusPart(second.Id, 20)]);

        LedgerTransaction refund = LedgerTransaction.OrderRefund(
            Guid.CreateVersion7(), "Duo", funding, this.customerPaid, this.customerBonus, Now);

        Assert.Equal(70, Sum(refund, AccountKind.CustomerPaid));
        Assert.Equal(-100, Sum(refund, AccountKind.Escrow));
        Assert.Equal(10, Entries(refund, AccountKind.CustomerBonus).Single(e => e.BonusGrantId == first.Id).Amount);
        Assert.Equal(20, Entries(refund, AccountKind.CustomerBonus).Single(e => e.BonusGrantId == second.Id).Amount);
    }

    [Fact]
    public void Payment_needs_one_customers_paid_and_bonus_accounts()
    {
        Account someoneElsesBonus = Account.OpenForProfile(AccountKind.CustomerBonus, Guid.CreateVersion7());

        Assert.Throws<ArgumentException>(() => LedgerTransaction.OrderPayment(
            Guid.CreateVersion7(), "Duo", new EscrowFunding(10, []), this.customerPaid, someoneElsesBonus, Now));
    }

    private static long Sum(LedgerTransaction transaction, AccountKind kind) =>
        Entries(transaction, kind).Sum(e => e.Amount);

    private static IEnumerable<LedgerEntry> Entries(LedgerTransaction transaction, AccountKind kind) =>
        transaction.Entries.Where(e => e.AccountKind == kind);

    private BonusGrant Grant(long amount, DateTimeOffset expiresAt) =>
        BonusGrant.Restore(Guid.CreateVersion7(), this.customerId, new Goin(amount), new Goin(amount), "Bonus", Now, expiresAt);
}
