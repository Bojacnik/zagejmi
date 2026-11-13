using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Npgsql;

using Zagejmi.Application.Wallets.Abstractions;
using Zagejmi.Domain.Wallets;
using Zagejmi.Infrastructure.Wallets.Ctx;
using Zagejmi.Infrastructure.Wallets.Records;

namespace Zagejmi.Infrastructure.Wallets.Repositories;

/// <summary>
///     PostgreSQL implementation of <see cref="IWalletRepository" />.
/// </summary>
public sealed class WalletRepository : IWalletRepository
{
    private const string UniqueViolation = "23505";

    private readonly IDbContextFactory<WalletsContext> contextFactory;
    private readonly TimeProvider timeProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WalletRepository" /> class.
    /// </summary>
    /// <param name="contextFactory">Creates short-lived contexts.</param>
    /// <param name="timeProvider">The clock.</param>
    public WalletRepository(IDbContextFactory<WalletsContext> contextFactory, TimeProvider timeProvider)
    {
        this.contextFactory = contextFactory;
        this.timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public async Task<Account> GetOrOpenAccountAsync(
        Guid profileId,
        AccountKind kind,
        CancellationToken cancellationToken = default)
    {
        await using WalletsContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        AccountRecord? existing = await FindAccountAsync(context, profileId, kind, cancellationToken);
        if (existing is not null)
        {
            return ToDomain(existing);
        }

        Account opened = Account.OpenForProfile(kind, profileId);
        string kindName = kind.ToString();
        DateTimeOffset now = this.timeProvider.GetUtcNow();

        // Two requests may open the same wallet at once; the unique index decides, and both read the winner.
        await context.Database.ExecuteSqlAsync(
            $"""
             INSERT INTO wallets."Accounts" ("Id", "Kind", "OwnerProfileId", "Balance", "AllowNegative", "CreatedAt")
             VALUES ({opened.Id}, {kindName}, {profileId}, 0, FALSE, {now})
             ON CONFLICT ("OwnerProfileId", "Kind") DO NOTHING
             """,
            cancellationToken);

        AccountRecord created = await FindAccountAsync(context, profileId, kind, cancellationToken)
                                ?? throw new InvalidOperationException($"Account {kind} of profile {profileId} was not opened.");

        return ToDomain(created);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Account>> GetAccountsAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        await using WalletsContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        List<AccountRecord> records = await context.Accounts
            .AsNoTracking()
            .Where(a => a.OwnerProfileId == profileId)
            .ToListAsync(cancellationToken);

        return [.. records.Select(ToDomain)];
    }

    /// <inheritdoc />
    public async Task<PostOutcome> PostAsync(
        LedgerTransaction transaction,
        BonusGrant? newGrant = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        await using WalletsContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);
        await using IDbContextTransaction dbTransaction = await context.Database.BeginTransactionAsync(cancellationToken);

        if (newGrant is not null)
        {
            // A new grant starts empty: the transaction's own entry credits it, like any later change.
            context.BonusGrants.Add(new BonusGrantRecord
            {
                Id = newGrant.Id,
                CustomerProfileId = newGrant.CustomerProfileId,
                Amount = newGrant.Amount.Amount,
                Remaining = 0,
                Reason = newGrant.Reason,
                GrantedAt = newGrant.GrantedAt,
                ExpiresAt = newGrant.ExpiresAt,
            });
        }

        context.Transactions.Add(ToRecord(transaction));

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: UniqueViolation })
        {
            await dbTransaction.RollbackAsync(cancellationToken);
            return PostOutcome.AlreadyPosted;
        }

        if (!await ApplyBalancesAsync(context, transaction, cancellationToken)
            || !await ApplyGrantsAsync(context, transaction, cancellationToken))
        {
            await dbTransaction.RollbackAsync(cancellationToken);
            return PostOutcome.InsufficientFunds;
        }

        await dbTransaction.CommitAsync(cancellationToken);
        return PostOutcome.Posted;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BonusGrant>> GetExpiredGrantsAsync(
        DateTimeOffset now,
        int take,
        CancellationToken cancellationToken = default)
    {
        await using WalletsContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        List<BonusGrantRecord> records = await context.BonusGrants
            .AsNoTracking()
            .Where(g => g.Remaining > 0 && g.ExpiresAt <= now)
            .OrderBy(g => g.ExpiresAt)
            .ThenBy(g => g.Id)
            .Take(take)
            .ToListAsync(cancellationToken);

        return [.. records.Select(ToDomain)];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BonusGrant>> GetActiveGrantsAsync(
        Guid customerProfileId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        await using WalletsContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        List<BonusGrantRecord> records = await context.BonusGrants
            .AsNoTracking()
            .Where(g => g.CustomerProfileId == customerProfileId && g.Remaining > 0 && g.ExpiresAt > now)
            .OrderBy(g => g.ExpiresAt)
            .ThenBy(g => g.Id)
            .ToListAsync(cancellationToken);

        return [.. records.Select(ToDomain)];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WalletHistoryItem>> GetHistoryAsync(
        IReadOnlyCollection<Guid> accountIds,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        await using WalletsContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        Guid[] ids = [.. accountIds];

        var changes = context.Entries
            .Where(e => ids.Contains(e.AccountId))
            .GroupBy(e => e.TransactionId)
            .Select(g => new
            {
                TransactionId = g.Key,
                Paid = g.Sum(e => e.Funding == Funding.Paid ? e.Amount : 0L),
                Bonus = g.Sum(e => e.Funding == Funding.Bonus ? e.Amount : 0L),
            });

        var rows = await context.Transactions
            .AsNoTracking()
            .Join(changes, t => t.Id, c => c.TransactionId, (t, c) => new
            {
                t.Id,
                t.Type,
                t.Description,
                t.CreatedAt,
                c.Paid,
                c.Bonus,
            })
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return [.. rows.Select(r => new WalletHistoryItem(r.Id, r.Type, r.Description, r.CreatedAt, r.Paid, r.Bonus))];
    }

    private static async Task<bool> ApplyBalancesAsync(
        WalletsContext context,
        LedgerTransaction transaction,
        CancellationToken cancellationToken)
    {
        // Rows are always locked in the same (ID) order, so two concurrent postings cannot deadlock.
        IEnumerable<(Guid AccountId, long Delta)> deltas = transaction.Entries
            .GroupBy(e => e.AccountId)
            .Select(g => (AccountId: g.Key, Delta: g.Sum(e => e.Amount)))
            .Where(d => d.Delta != 0)
            .OrderBy(d => d.AccountId);

        foreach ((Guid accountId, long delta) in deltas)
        {
            // A single UPDATE both locks the row and checks the balance, so concurrent postings never lose an update
            // and an account that does not allow it can never go negative.
            int updated = await context.Accounts
                .Where(a => a.Id == accountId && (a.AllowNegative || a.Balance + delta >= 0))
                .ExecuteUpdateAsync(set => set.SetProperty(a => a.Balance, a => a.Balance + delta), cancellationToken);

            if (updated == 0)
            {
                return false;
            }
        }

        return true;
    }

    private static async Task<bool> ApplyGrantsAsync(
        WalletsContext context,
        LedgerTransaction transaction,
        CancellationToken cancellationToken)
    {
        // Only Goins entering or leaving a customer's bonus account change what is left of a grant. Bonus-funded
        // entries elsewhere (escrow, player earnings) keep the grant ID purely to trace where the Goins came from.
        IEnumerable<(Guid GrantId, long Delta)> deltas = transaction.Entries
            .Where(e => e.AccountKind == AccountKind.CustomerBonus)
            .GroupBy(e => e.BonusGrantId!.Value)
            .Select(g => (GrantId: g.Key, Delta: g.Sum(e => e.Amount)))
            .Where(d => d.Delta != 0)
            .OrderBy(d => d.GrantId);

        foreach ((Guid grantId, long delta) in deltas)
        {
            int updated = await context.BonusGrants
                .Where(g => g.Id == grantId && g.Remaining + delta >= 0 && g.Remaining + delta <= g.Amount)
                .ExecuteUpdateAsync(set => set.SetProperty(g => g.Remaining, g => g.Remaining + delta), cancellationToken);

            if (updated == 0)
            {
                return false;
            }
        }

        return true;
    }

    private static Task<AccountRecord?> FindAccountAsync(
        WalletsContext context,
        Guid profileId,
        AccountKind kind,
        CancellationToken cancellationToken)
    {
        return context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.OwnerProfileId == profileId && a.Kind == kind, cancellationToken);
    }

    private static LedgerTransactionRecord ToRecord(LedgerTransaction transaction)
    {
        return new LedgerTransactionRecord
        {
            Id = transaction.Id,
            Type = transaction.Type,
            CreatedAt = transaction.CreatedAt,
            Description = transaction.Description,
            IdempotencyKey = transaction.IdempotencyKey,
            Entries =
            [
                .. transaction.Entries.Select(e => new LedgerEntryRecord
                {
                    TransactionId = transaction.Id,
                    AccountId = e.AccountId,
                    AccountKind = e.AccountKind,
                    Amount = e.Amount,
                    Funding = e.Funding,
                    BonusGrantId = e.BonusGrantId,
                }),
            ],
        };
    }

    private static Account ToDomain(AccountRecord record)
    {
        return Account.Restore(record.Id, record.Kind, record.OwnerProfileId, record.Balance);
    }

    private static BonusGrant ToDomain(BonusGrantRecord record)
    {
        return BonusGrant.Restore(
            record.Id,
            record.CustomerProfileId,
            new Goin(record.Amount),
            new Goin(record.Remaining),
            record.Reason,
            record.GrantedAt,
            record.ExpiresAt);
    }
}
