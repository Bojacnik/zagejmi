using System;

using Xunit;

using Zagejmi.Domain.Wallets;

namespace Zagejmi.Domain.Tests.Wallets;

public sealed class GoinAndAccountTests
{
    [Fact]
    public void Goin_cannot_be_negative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Goin(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Goin(5) - new Goin(6));
        Assert.Equal(new Goin(1), new Goin(6) - new Goin(5));
    }

    [Fact]
    public void User_account_cannot_go_negative()
    {
        Account paid = Account.OpenForProfile(AccountKind.CustomerPaid, Guid.CreateVersion7());

        paid.Apply(new LedgerEntry(paid.Id, AccountKind.CustomerPaid, 10, Funding.Paid, null));
        Assert.Throws<InvalidOperationException>(
            () => paid.Apply(new LedgerEntry(paid.Id, AccountKind.CustomerPaid, -11, Funding.Paid, null)));

        Assert.Equal(10, paid.Balance);
    }

    [Fact]
    public void Promo_budget_can_go_negative()
    {
        Account promo = Account.Restore(SystemAccounts.PromoBudgetId, AccountKind.PromoBudget, null, 0);

        promo.Apply(new LedgerEntry(promo.Id, AccountKind.PromoBudget, -50, Funding.Bonus, Guid.CreateVersion7()));

        Assert.Equal(-50, promo.Balance);
    }

    [Fact]
    public void System_accounts_cannot_be_opened_for_a_profile()
    {
        Assert.Throws<ArgumentException>(() => Account.OpenForProfile(AccountKind.Escrow, Guid.CreateVersion7()));
    }

    [Fact]
    public void Entry_for_another_account_is_rejected()
    {
        Account paid = Account.OpenForProfile(AccountKind.CustomerPaid, Guid.CreateVersion7());

        Assert.Throws<ArgumentException>(
            () => paid.Apply(new LedgerEntry(Guid.CreateVersion7(), AccountKind.CustomerPaid, 10, Funding.Paid, null)));
    }

    [Fact]
    public void Bonus_grant_remaining_stays_within_its_amount()
    {
        Guid profileId = Guid.CreateVersion7();
        Account bonus = Account.OpenForProfile(AccountKind.CustomerBonus, profileId);
        BonusGrant grant = BonusGrant.Create(profileId, new Goin(50), "Bonus", DateTimeOffset.UtcNow, TimeSpan.FromDays(90));

        Assert.Throws<InvalidOperationException>(
            () => grant.Apply(new LedgerEntry(bonus.Id, AccountKind.CustomerBonus, 1, Funding.Bonus, grant.Id)));
        Assert.Throws<InvalidOperationException>(
            () => grant.Apply(new LedgerEntry(bonus.Id, AccountKind.CustomerBonus, -51, Funding.Bonus, grant.Id)));

        grant.Apply(new LedgerEntry(bonus.Id, AccountKind.CustomerBonus, -30, Funding.Bonus, grant.Id));
        Assert.Equal(new Goin(20), grant.Remaining);
    }

    [Fact]
    public void Bonus_grant_expires_exactly_at_its_expiry()
    {
        DateTimeOffset now = new(2026, 11, 13, 0, 0, 0, TimeSpan.Zero);
        BonusGrant grant = BonusGrant.Create(Guid.CreateVersion7(), new Goin(1), "Bonus", now, BonusGrant.DefaultLifetime);

        Assert.Equal(now.AddDays(90), grant.ExpiresAt);
        Assert.False(grant.IsExpired(grant.ExpiresAt.AddTicks(-1)));
        Assert.True(grant.IsExpired(grant.ExpiresAt));
    }
}
