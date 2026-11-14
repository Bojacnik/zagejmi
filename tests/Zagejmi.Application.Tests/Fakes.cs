using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Auth.Passwords;
using Zagejmi.Application.Auth.Tokens;
using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Application.Auth.Users.EmailVerification;
using Zagejmi.Application.Mail.Abstractions;
using Zagejmi.Application.Profiles;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Application.Profiles.Reviews;
using Zagejmi.Application.Profiles.Reviews.Abstractions;
using Zagejmi.Application.Profiles.SearchServices;
using Zagejmi.Application.Wallets;
using Zagejmi.Application.Wallets.Abstractions;
using Zagejmi.Application.Wallets.GrantReward;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Profiles.Customers;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Domain.Wallets;

namespace Zagejmi.Application.Tests;

/// <summary>
///     Everything a handler test needs, wired together in memory.
/// </summary>
internal sealed class TestWorld
{
    public TestWorld(long signUpBonus = 50)
    {
        this.WalletOptions = Options.Create(new WalletOptions { SignUpBonus = signUpBonus });
        this.Wallets = new FakeWalletRepository();
        this.Mailer = new EmailVerificationMailer(
            this.Tokens,
            new FakeLinkFactory(),
            this.Email,
            this.Clock,
            this.WalletOptions,
            NullLogger<EmailVerificationMailer>.Instance);
        this.GrantReward = new GrantRewardHandler(this.Wallets, this.Clock, this.WalletOptions);
    }

    public FakeClock Clock { get; } = new(new DateTimeOffset(2026, 11, 13, 12, 0, 0, TimeSpan.Zero));

    public FakeUserRepository Users { get; } = new();

    public FakeProfileRepository Profiles { get; } = new();

    public FakePlayerReviewRepository Reviews { get; } = new();

    public FakeWalletRepository Wallets { get; }

    public FakeEmailSender Email { get; } = new();

    public FakeTokenService Tokens { get; } = new();

    public IOptions<WalletOptions> WalletOptions { get; }

    public EmailVerificationMailer Mailer { get; }

    public GrantRewardHandler GrantReward { get; }

    /// <summary>
    ///     Adds a user with a customer profile; the password is always "heslo123".
    /// </summary>
    public (User User, CustomerProfile Customer) AddUser(string userName, bool verified)
    {
        User user = new(Guid.CreateVersion7())
        {
            AuthCredentials = new AuthCredentials(
                userName,
                "hash:heslo123",
                $"{userName}@example.test",
                verified ? this.Clock.GetUtcNow() : null),
        };

        CustomerProfile customer = new(Guid.CreateVersion7(), user.Id, userName);
        this.Users.Items.Add(user);
        this.Profiles.Customers.Add(customer);
        return (user, customer);
    }
}

internal sealed class FakeClock : TimeProvider
{
    private DateTimeOffset now;

    public FakeClock(DateTimeOffset now)
    {
        this.now = now;
    }

    public override DateTimeOffset GetUtcNow()
    {
        return this.now;
    }

    public void Advance(TimeSpan by)
    {
        this.now += by;
    }
}

internal sealed class FakeUserRepository : IUserRepository
{
    public List<User> Items { get; } = [];

    public int Updates { get; private set; }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(this.Items.FirstOrDefault(u => u.Id == id));
    }

    public Task<Guid> AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        this.Items.Add(entity);
        return Task.FromResult(entity.Id);
    }

    public Task UpdateAsync(User entity, CancellationToken cancellationToken = default)
    {
        this.Updates++;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        this.Items.RemoveAll(u => u.Id == entityId);
        return Task.CompletedTask;
    }

    public Task<bool> IsUserNameOrEmailTakenAsync(string userName, string email, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(this.Items.Any(u =>
            string.Equals(u.AuthCredentials.UserName, userName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(u.AuthCredentials.Email, email, StringComparison.OrdinalIgnoreCase)));
    }

    public Task<User?> GetByUserNameOrEmailAsync(string userNameOrEmail, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(this.Items.FirstOrDefault(u =>
            string.Equals(u.AuthCredentials.UserName, userNameOrEmail, StringComparison.OrdinalIgnoreCase)
            || string.Equals(u.AuthCredentials.Email, userNameOrEmail, StringComparison.OrdinalIgnoreCase)));
    }
}

internal sealed class FakeProfileRepository : IProfileRepository
{
    public List<CustomerProfile> Customers { get; } = [];

    public List<PlayerProfile> Players { get; } = [];

    public Task<CustomerProfile?> GetCustomerByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(this.Customers.FirstOrDefault(c => c.UserId == userId));
    }

    public Task<PlayerProfile?> GetPlayerByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(this.Players.FirstOrDefault(p => p.UserId == userId));
    }

    public Task AddCustomerAsync(CustomerProfile profile, CancellationToken cancellationToken = default)
    {
        this.Customers.Add(profile);
        return Task.CompletedTask;
    }

    public Task<PlayerProfile?> GetPlayerByIdAsync(Guid profileId, CancellationToken cancellationToken = default) =>
        Task.FromResult(this.Players.FirstOrDefault(p => p.Id == profileId));

    public Task<bool> IsDisplayNameTakenAsync(string displayName, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task AddPlayerAsync(PlayerProfile profile, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<PagedResult<PlayerProfile>> SearchPlayersAsync(
        string? search,
        GenderGroup? gender,
        int skip,
        int take,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<PagedResult<ServiceListItem>> SearchServicesAsync(
        ServiceSearchFilter filter,
        int skip,
        int take,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task UpdateCustomerAsync(CustomerProfile profile, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task UpdatePlayerAsync(PlayerProfile profile, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<IReadOnlyList<PlayerService>> GetPlayerServicesAsync(
        Guid playerProfileId,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<int> CountPlayerServicesAsync(Guid playerProfileId, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task AddPlayerServiceAsync(PlayerService service, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<PlayerService?> GetPlayerServiceAsync(Guid serviceId, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task UpdatePlayerServiceAsync(PlayerService service, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task DeletePlayerServiceAsync(Guid serviceId, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();
}

/// <summary>
///     In-memory ledger with the same rules as the PostgreSQL repository.
/// </summary>
internal sealed class FakeWalletRepository : IWalletRepository
{
    public List<Account> Accounts { get; } =
    [
        Account.Restore(SystemAccounts.PromoBudgetId, AccountKind.PromoBudget, null, 0),
    ];

    public List<LedgerTransaction> Posted { get; } = [];

    public List<BonusGrant> Grants { get; } = [];

    public Task<Account> GetOrOpenAccountAsync(Guid profileId, AccountKind kind, CancellationToken cancellationToken = default)
    {
        Account? account = this.Accounts.FirstOrDefault(a => a.OwnerProfileId == profileId && a.Kind == kind);
        if (account is null)
        {
            account = Account.OpenForProfile(kind, profileId);
            this.Accounts.Add(account);
        }

        return Task.FromResult(account);
    }

    public Task<IReadOnlyList<Account>> GetAccountsAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Account>>([.. this.Accounts.Where(a => a.OwnerProfileId == profileId)]);
    }

    public Task<PostOutcome> PostAsync(
        LedgerTransaction transaction,
        BonusGrant? newGrant = null,
        CancellationToken cancellationToken = default)
    {
        if (this.Posted.Any(t => t.Type == transaction.Type && t.IdempotencyKey == transaction.IdempotencyKey))
        {
            return Task.FromResult(PostOutcome.AlreadyPosted);
        }

        List<BonusGrant> grants = [.. this.Grants];
        if (newGrant is not null)
        {
            grants.Add(BonusGrant.Restore(
                newGrant.Id,
                newGrant.CustomerProfileId,
                newGrant.Amount,
                Goin.Zero,
                newGrant.Reason,
                newGrant.GrantedAt,
                newGrant.ExpiresAt));
        }

        // Check everything first so a failed posting changes nothing.
        foreach (IGrouping<Guid, LedgerEntry> account in transaction.Entries.GroupBy(e => e.AccountId))
        {
            Account target = this.Accounts.Single(a => a.Id == account.Key);
            if (target.IsUserAccount && target.Balance + account.Sum(e => e.Amount) < 0)
            {
                return Task.FromResult(PostOutcome.InsufficientFunds);
            }
        }

        foreach (IGrouping<Guid, LedgerEntry> grant in transaction.Entries
                     .Where(e => e.AccountKind == AccountKind.CustomerBonus)
                     .GroupBy(e => e.BonusGrantId!.Value))
        {
            BonusGrant target = grants.Single(g => g.Id == grant.Key);
            long remaining = target.Remaining.Amount + grant.Sum(e => e.Amount);
            if (remaining < 0 || remaining > target.Amount.Amount)
            {
                return Task.FromResult(PostOutcome.InsufficientFunds);
            }
        }

        foreach (LedgerEntry entry in transaction.Entries)
        {
            this.Accounts.Single(a => a.Id == entry.AccountId).Apply(entry);
            if (entry.AccountKind == AccountKind.CustomerBonus)
            {
                grants.Single(g => g.Id == entry.BonusGrantId).Apply(entry);
            }
        }

        this.Grants.Clear();
        this.Grants.AddRange(grants);
        this.Posted.Add(transaction);
        return Task.FromResult(PostOutcome.Posted);
    }

    public Task<IReadOnlyList<BonusGrant>> GetExpiredGrantsAsync(
        DateTimeOffset now,
        int take,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<BonusGrant>>(
            [.. this.Grants.Where(g => g.Remaining.Amount > 0 && g.ExpiresAt <= now).OrderBy(g => g.ExpiresAt).Take(take)]);
    }

    public Task<IReadOnlyList<BonusGrant>> GetActiveGrantsAsync(
        Guid customerProfileId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<BonusGrant>>(
        [
            .. this.Grants
                .Where(g => g.CustomerProfileId == customerProfileId && g.Remaining.Amount > 0 && g.ExpiresAt > now)
                .OrderBy(g => g.ExpiresAt),
        ]);
    }

    public Task<IReadOnlyList<WalletHistoryItem>> GetHistoryAsync(
        IReadOnlyCollection<Guid> accountIds,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<WalletHistoryItem>>(
        [
            .. this.Posted
                .Where(t => t.Entries.Any(e => accountIds.Contains(e.AccountId)))
                .OrderByDescending(t => t.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(t => new WalletHistoryItem(
                    t.Id,
                    t.Type,
                    t.Description,
                    t.CreatedAt,
                    t.Entries.Where(e => accountIds.Contains(e.AccountId) && e.Funding == Funding.Paid).Sum(e => e.Amount),
                    t.Entries.Where(e => accountIds.Contains(e.AccountId) && e.Funding == Funding.Bonus).Sum(e => e.Amount))),
        ]);
    }
}

internal sealed class FakeEmailSender : IEmailSender
{
    public List<(string To, string Subject, string Body)> Sent { get; } = [];

    public bool Fail { get; set; }

    public Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        if (this.Fail)
        {
            throw new InvalidOperationException("SMTP is down.");
        }

        this.Sent.Add((toEmail, subject, htmlBody));
        return Task.CompletedTask;
    }
}

internal sealed class FakeTokenService : IEmailVerificationTokenService
{
    private int counter;

    public string LastToken { get; private set; } = string.Empty;

    public string GenerateToken()
    {
        this.LastToken = $"token-{++this.counter}";
        return this.LastToken;
    }

    public string HashToken(string token)
    {
        return $"hash:{token}";
    }
}

internal sealed class FakeLinkFactory : IEmailVerificationLinkFactory
{
    public string Create(Guid userId, string token)
    {
        return $"https://zagejmi.test/verify-email?userId={userId}&token={token}";
    }
}

internal sealed class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return $"hash:{password}";
    }

    public bool Verify(string passwordHash, string password)
    {
        return passwordHash == $"hash:{password}";
    }
}

internal sealed class FakeAccessTokens : ITokenService, IRefreshTokenRepository
{
    public TimeSpan RefreshTokenLifetime => TimeSpan.FromDays(30);

    public string GenerateToken(User user)
    {
        return $"jwt:{user.Id}";
    }

    public string GenerateRefreshToken()
    {
        return "refresh";
    }

    public string HashRefreshToken(string refreshToken)
    {
        return $"hash:{refreshToken}";
    }

    public bool TryGetUserId(string jwtToken, out Guid userId)
    {
        userId = Guid.Empty;
        return false;
    }

    public Task AddAsync(Guid userId, string tokenHash, DateTimeOffset expiresAt, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

/// <summary>
///     In-memory reviews with the same upsert semantics as the database (one per author and player; an update keeps
///     the original ID and creation time).
/// </summary>
internal sealed class FakePlayerReviewRepository : IPlayerReviewRepository
{
    public List<PlayerReview> Items { get; } = [];

    public Func<Guid, (string Name, Guid? ImageId)> Authors { get; set; } = _ => ("Autor", null);

    public Task<PlayerReview?> GetAsync(Guid playerProfileId, Guid authorProfileId, CancellationToken cancellationToken = default) =>
        Task.FromResult(this.Items.FirstOrDefault(r => r.PlayerProfileId == playerProfileId && r.AuthorProfileId == authorProfileId));

    public Task UpsertAsync(PlayerReview review, CancellationToken cancellationToken = default)
    {
        int index = this.Items.FindIndex(r => r.PlayerProfileId == review.PlayerProfileId && r.AuthorProfileId == review.AuthorProfileId);
        if (index < 0)
        {
            this.Items.Add(review);
        }
        else
        {
            this.Items[index] = this.Items[index] with
            {
                Rating = review.Rating,
                Comment = review.Comment,
                UpdatedAt = review.UpdatedAt,
            };
        }

        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(Guid playerProfileId, Guid authorProfileId, CancellationToken cancellationToken = default) =>
        Task.FromResult(this.Items.RemoveAll(r => r.PlayerProfileId == playerProfileId && r.AuthorProfileId == authorProfileId) > 0);

    public Task<PagedResult<PlayerReviewItem>> GetForPlayerAsync(
        Guid playerProfileId,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        List<PlayerReview> all = this.Items
            .Where(r => r.PlayerProfileId == playerProfileId)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();
        List<PlayerReviewItem> page = all.Skip(skip).Take(take).Select(r =>
        {
            (string name, Guid? image) = this.Authors(r.AuthorProfileId);
            return new PlayerReviewItem(r.AuthorProfileId, name, image, r.Rating, r.Comment, r.CreatedAt, r.UpdatedAt);
        }).ToList();
        return Task.FromResult(new PagedResult<PlayerReviewItem>(page, all.Count));
    }

    public Task<IReadOnlyDictionary<Guid, RatingSummary>> GetSummariesAsync(
        IReadOnlyCollection<Guid> playerProfileIds,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyDictionary<Guid, RatingSummary> result = this.Items
            .Where(r => playerProfileIds.Contains(r.PlayerProfileId))
            .GroupBy(r => r.PlayerProfileId)
            .ToDictionary(g => g.Key, g => new RatingSummary(g.Average(r => (double)r.Rating), g.Count()));
        return Task.FromResult(result);
    }
}
