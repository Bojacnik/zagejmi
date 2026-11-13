using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Xunit;

using Zagejmi.Application.Wallets.Abstractions;
using Zagejmi.Domain.Wallets;
using Zagejmi.Infrastructure.Wallets.Ctx;
using Zagejmi.Infrastructure.Wallets.Repositories;

namespace Zagejmi.Infrastructure.Tests.Wallets;

/// <summary>
///     Runs against a real PostgreSQL. Set ZAGEJMI_TEST_POSTGRES to a connection string of a server where the user may
///     create databases (e.g. the docker-compose one); each test class run uses its own throw-away database.
/// </summary>
public sealed class WalletRepositoryTests : IAsyncLifetime
{
    private const string ConnectionVariable = "ZAGEJMI_TEST_POSTGRES";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static readonly DateTimeOffset Now = new(2026, 11, 13, 12, 0, 0, TimeSpan.Zero);

    private readonly string? serverConnection = Environment.GetEnvironmentVariable(ConnectionVariable);
    private readonly string databaseName = $"zagejmi_test_{Guid.NewGuid():N}";
    private TestContextFactory? factory;

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
    public async Task Opening_the_same_account_concurrently_yields_one_account()
    {
        WalletRepository repository = this.CreateRepository();
        Guid profileId = Guid.CreateVersion7();

        Account[] opened = await Task.WhenAll(Enumerable.Range(0, 10).Select(
            _ => repository.GetOrOpenAccountAsync(profileId, AccountKind.CustomerBonus, Ct)));

        Assert.Single(opened.Select(a => a.Id).Distinct());
        Assert.Single(await repository.GetAccountsAsync(profileId, Ct));
    }

    [Fact]
    public async Task Reward_credits_the_wallet_and_grant_once_even_when_posted_twice()
    {
        WalletRepository repository = this.CreateRepository();
        Guid profileId = Guid.CreateVersion7();
        Account bonus = await repository.GetOrOpenAccountAsync(profileId, AccountKind.CustomerBonus, Ct);

        BonusGrant first = NewGrant(profileId, 50);
        BonusGrant retry = NewGrant(profileId, 50);

        Assert.Equal(PostOutcome.Posted, await repository.PostAsync(LedgerTransaction.Reward(first, bonus, "signup:1"), first, Ct));
        Assert.Equal(PostOutcome.AlreadyPosted, await repository.PostAsync(LedgerTransaction.Reward(retry, bonus, "signup:1"), retry, Ct));

        Assert.Equal(50, (await repository.GetAccountsAsync(profileId, Ct)).Single().Balance);
        BonusGrant stored = Assert.Single(await repository.GetActiveGrantsAsync(profileId, Now, Ct));
        Assert.Equal(first.Id, stored.Id);
        Assert.Equal(new Goin(50), stored.Remaining);
        await this.AssertLedgerConsistentAsync();
    }

    [Fact]
    public async Task Concurrent_rewards_never_lose_an_update()
    {
        WalletRepository repository = this.CreateRepository();
        Guid profileId = Guid.CreateVersion7();
        Account bonus = await repository.GetOrOpenAccountAsync(profileId, AccountKind.CustomerBonus, Ct);

        PostOutcome[] outcomes = await Task.WhenAll(Enumerable.Range(0, 20).Select(i =>
        {
            BonusGrant grant = NewGrant(profileId, 10);
            return repository.PostAsync(LedgerTransaction.Reward(grant, bonus, $"reward:{i}"), grant, Ct);
        }));

        Assert.All(outcomes, o => Assert.Equal(PostOutcome.Posted, o));
        Assert.Equal(200, (await repository.GetAccountsAsync(profileId, Ct)).Single().Balance);
        await this.AssertLedgerConsistentAsync();
    }

    [Fact]
    public async Task Posting_that_would_overdraw_a_wallet_changes_nothing()
    {
        WalletRepository repository = this.CreateRepository();
        Guid profileId = Guid.CreateVersion7();
        Account bonus = await repository.GetOrOpenAccountAsync(profileId, AccountKind.CustomerBonus, Ct);
        BonusGrant grant = NewGrant(profileId, 50);
        await repository.PostAsync(LedgerTransaction.Reward(grant, bonus, "k"), grant, Ct);

        // A stale copy of the grant that believes 60 are left.
        BonusGrant stale = BonusGrant.Restore(grant.Id, profileId, new Goin(60), new Goin(60), grant.Reason, Now, Now.AddDays(1));
        PostOutcome outcome = await repository.PostAsync(LedgerTransaction.BonusExpiry(stale, bonus, Now.AddDays(2)), null, Ct);

        Assert.Equal(PostOutcome.InsufficientFunds, outcome);
        Assert.Equal(50, (await repository.GetAccountsAsync(profileId, Ct)).Single().Balance);
        await this.AssertLedgerConsistentAsync();
    }

    [Fact]
    public async Task Expiry_returns_the_rest_and_shows_in_the_history()
    {
        WalletRepository repository = this.CreateRepository();
        Guid profileId = Guid.CreateVersion7();
        Account bonus = await repository.GetOrOpenAccountAsync(profileId, AccountKind.CustomerBonus, Ct);
        BonusGrant grant = NewGrant(profileId, 50);
        await repository.PostAsync(LedgerTransaction.Reward(grant, bonus, "k"), grant, Ct);

        DateTimeOffset later = grant.ExpiresAt.AddMinutes(1);
        BonusGrant due = Assert.Single(await repository.GetExpiredGrantsAsync(later, 10, Ct), g => g.CustomerProfileId == profileId);
        Assert.Equal(PostOutcome.Posted, await repository.PostAsync(LedgerTransaction.BonusExpiry(due, bonus, later), null, Ct));

        Assert.Equal(0, (await repository.GetAccountsAsync(profileId, Ct)).Single().Balance);
        Assert.DoesNotContain(await repository.GetExpiredGrantsAsync(later, 100, Ct), g => g.CustomerProfileId == profileId);

        IReadOnlyList<WalletHistoryItem> history = await repository.GetHistoryAsync([bonus.Id], 0, 10, Ct);
        Assert.Collection(
            history,
            newest =>
            {
                Assert.Equal(LedgerTransactionType.BonusExpiry, newest.Type);
                Assert.Equal(-50, newest.BonusChange);
                Assert.Equal(0, newest.PaidChange);
            },
            oldest =>
            {
                Assert.Equal(LedgerTransactionType.Reward, oldest.Type);
                Assert.Equal(50, oldest.BonusChange);
            });
        await this.AssertLedgerConsistentAsync();
    }

    private static BonusGrant NewGrant(Guid profileId, long amount)
    {
        return BonusGrant.Create(profileId, new Goin(amount), "Bonus za registraci", Now, BonusGrant.DefaultLifetime);
    }

    private WalletRepository CreateRepository()
    {
        Assert.SkipWhen(this.factory is null, $"Set {ConnectionVariable} to run the PostgreSQL wallet tests.");
        return new WalletRepository(this.factory!, TimeProvider.System);
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
                a.Id,
                a.Kind,
                a.OwnerProfileId,
                a.Balance,
                Sum = context.Entries.Where(e => e.AccountId == a.Id).Sum(e => (long?)e.Amount) ?? 0,
                Grants = context.BonusGrants.Where(g => g.CustomerProfileId == a.OwnerProfileId).Sum(g => (long?)g.Remaining) ?? 0,
            })
            .ToListAsync(Ct);

        Assert.All(balances, b => Assert.Equal(b.Sum, b.Balance));
        Assert.All(balances.Where(b => b.Kind == AccountKind.CustomerBonus), b => Assert.Equal(b.Grants, b.Balance));
    }

    private sealed class TestContextFactory : IDbContextFactory<WalletsContext>
    {
        private readonly DbContextOptions<WalletsContext> options;

        public TestContextFactory(string connection)
        {
            this.options = new DbContextOptionsBuilder<WalletsContext>()
                .UseNpgsql(connection, npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "wallets"))
                .Options;
        }

        public WalletsContext CreateDbContext()
        {
            return new WalletsContext(this.options);
        }
    }
}
