using System;
using System.Linq;

using Xunit;

using Zagejmi.Domain.Wallets;

namespace Zagejmi.Domain.Tests.Wallets;

public sealed class LedgerTransactionTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 13, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid CustomerProfileId = Guid.CreateVersion7();

    [Fact]
    public void Reward_moves_the_grant_from_the_promo_budget_to_the_bonus_account()
    {
        Account bonus = Account.OpenForProfile(AccountKind.CustomerBonus, CustomerProfileId);
        BonusGrant grant = NewGrant(50);

        LedgerTransaction transaction = LedgerTransaction.Reward(grant, bonus, "signup-bonus:1");

        Assert.Equal(LedgerTransactionType.Reward, transaction.Type);
        Assert.Equal("Bonus za registraci", transaction.Description);
        Assert.Collection(
            transaction.Entries,
            e => Assert.Equal(new LedgerEntry(SystemAccounts.PromoBudgetId, AccountKind.PromoBudget, -50, Funding.Bonus, grant.Id), e),
            e => Assert.Equal(new LedgerEntry(bonus.Id, AccountKind.CustomerBonus, 50, Funding.Bonus, grant.Id), e));
    }

    [Fact]
    public void Every_transaction_balances_per_funding_type()
    {
        Account bonus = Account.OpenForProfile(AccountKind.CustomerBonus, CustomerProfileId);
        BonusGrant grant = NewGrant(50);

        LedgerTransaction reward = LedgerTransaction.Reward(grant, bonus, "k");
        LedgerTransaction expiry = LedgerTransaction.BonusExpiry(grant, bonus, grant.ExpiresAt);

        foreach (LedgerTransaction transaction in new[] { reward, expiry })
        {
            Assert.All(
                transaction.Entries.GroupBy(e => e.Funding),
                group => Assert.Equal(0, group.Sum(e => e.Amount)));
        }
    }

    [Fact]
    public void Reward_rejects_a_bonus_account_of_another_profile()
    {
        Account someoneElses = Account.OpenForProfile(AccountKind.CustomerBonus, Guid.CreateVersion7());

        Assert.Throws<ArgumentException>(() => LedgerTransaction.Reward(NewGrant(50), someoneElses, "k"));
    }

    [Fact]
    public void Reward_rejects_the_paid_account()
    {
        Account paid = Account.OpenForProfile(AccountKind.CustomerPaid, CustomerProfileId);

        Assert.Throws<ArgumentException>(() => LedgerTransaction.Reward(NewGrant(50), paid, "k"));
    }

    [Fact]
    public void Reward_needs_an_idempotency_key()
    {
        Account bonus = Account.OpenForProfile(AccountKind.CustomerBonus, CustomerProfileId);

        Assert.Throws<ArgumentException>(() => LedgerTransaction.Reward(NewGrant(50), bonus, " "));
    }

    [Fact]
    public void BonusExpiry_returns_what_is_left_to_the_promo_budget()
    {
        Account bonus = Account.OpenForProfile(AccountKind.CustomerBonus, CustomerProfileId);
        BonusGrant grant = BonusGrant.Restore(
            Guid.CreateVersion7(),
            CustomerProfileId,
            new Goin(50),
            new Goin(20),
            "Bonus za registraci",
            Now,
            Now.AddDays(90));

        LedgerTransaction transaction = LedgerTransaction.BonusExpiry(grant, bonus, Now.AddDays(90));

        Assert.Equal(LedgerTransactionType.BonusExpiry, transaction.Type);
        Assert.Equal("Vypršení bonusu: Bonus za registraci", transaction.Description);
        Assert.Collection(
            transaction.Entries,
            e => Assert.Equal(new LedgerEntry(bonus.Id, AccountKind.CustomerBonus, -20, Funding.Bonus, grant.Id), e),
            e => Assert.Equal(new LedgerEntry(SystemAccounts.PromoBudgetId, AccountKind.PromoBudget, 20, Funding.Bonus, grant.Id), e));
    }

    [Fact]
    public void BonusExpiry_is_refused_before_the_expiry()
    {
        Account bonus = Account.OpenForProfile(AccountKind.CustomerBonus, CustomerProfileId);
        BonusGrant grant = NewGrant(50);

        Assert.Throws<InvalidOperationException>(
            () => LedgerTransaction.BonusExpiry(grant, bonus, grant.ExpiresAt.AddTicks(-1)));
    }

    [Fact]
    public void BonusExpiry_is_refused_when_nothing_is_left()
    {
        Account bonus = Account.OpenForProfile(AccountKind.CustomerBonus, CustomerProfileId);
        BonusGrant grant = BonusGrant.Restore(
            Guid.CreateVersion7(),
            CustomerProfileId,
            new Goin(50),
            Goin.Zero,
            "Bonus",
            Now,
            Now.AddDays(1));

        Assert.Throws<InvalidOperationException>(() => LedgerTransaction.BonusExpiry(grant, bonus, Now.AddDays(2)));
    }

    [Fact]
    public void BonusExpiry_key_changes_with_the_expiry_so_a_reopened_grant_can_expire_again()
    {
        Account bonus = Account.OpenForProfile(AccountKind.CustomerBonus, CustomerProfileId);
        Guid grantId = Guid.CreateVersion7();
        BonusGrant first = BonusGrant.Restore(grantId, CustomerProfileId, new Goin(50), new Goin(50), "B", Now, Now.AddDays(1));
        BonusGrant extended = BonusGrant.Restore(grantId, CustomerProfileId, new Goin(50), new Goin(10), "B", Now, Now.AddDays(9));

        string firstKey = LedgerTransaction.BonusExpiry(first, bonus, Now.AddDays(10)).IdempotencyKey;
        string secondKey = LedgerTransaction.BonusExpiry(extended, bonus, Now.AddDays(10)).IdempotencyKey;

        Assert.NotEqual(firstKey, secondKey);
    }

    [Fact]
    public void Applying_a_reward_and_its_expiry_leaves_the_wallet_empty_and_the_budget_even()
    {
        Account bonus = Account.OpenForProfile(AccountKind.CustomerBonus, CustomerProfileId);
        Account promo = Account.Restore(SystemAccounts.PromoBudgetId, AccountKind.PromoBudget, null, 0);
        BonusGrant grant = AsStoredBeforeItsReward(NewGrant(50));

        Post(LedgerTransaction.Reward(grant, bonus, "k"), bonus, promo, grant);
        Assert.Equal(50, bonus.Balance);
        Assert.Equal(-50, promo.Balance);
        Assert.Equal(new Goin(50), grant.Remaining);

        Post(LedgerTransaction.BonusExpiry(grant, bonus, grant.ExpiresAt), bonus, promo, grant);
        Assert.Equal(0, bonus.Balance);
        Assert.Equal(0, promo.Balance);
        Assert.Equal(Goin.Zero, grant.Remaining);
    }

    private static BonusGrant NewGrant(long amount)
    {
        return BonusGrant.Create(CustomerProfileId, new Goin(amount), "Bonus za registraci", Now, BonusGrant.DefaultLifetime);
    }

    private static BonusGrant AsStoredBeforeItsReward(BonusGrant created)
    {
        // A new grant starts empty in storage and is credited by its reward transaction; mirror that here.
        return BonusGrant.Restore(
            created.Id,
            created.CustomerProfileId,
            created.Amount,
            Goin.Zero,
            created.Reason,
            created.GrantedAt,
            created.ExpiresAt);
    }

    private static void Post(LedgerTransaction transaction, Account bonus, Account promo, BonusGrant grant)
    {
        foreach (LedgerEntry entry in transaction.Entries)
        {
            (entry.AccountId == bonus.Id ? bonus : promo).Apply(entry);
            if (entry.AccountKind == AccountKind.CustomerBonus)
            {
                grant.Apply(entry);
            }
        }
    }
}
