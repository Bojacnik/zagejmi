using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Xunit;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Profiles.Reviews;
using Zagejmi.Domain.Profiles.Customers;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Infrastructure.Auth.Users.Ctx;
using Zagejmi.Infrastructure.Profiles.Ctx;
using Zagejmi.Infrastructure.Profiles.Repositories;

namespace Zagejmi.Infrastructure.Tests.Profiles;

/// <summary>
///     Runs the review queries (including the raw upsert) against a real PostgreSQL (set ZAGEJMI_TEST_POSTGRES, see the
///     wallet tests); each run uses its own throw-away database.
/// </summary>
public sealed class PlayerReviewRepositoryTests : IAsyncLifetime
{
    private const string ConnectionVariable = "ZAGEJMI_TEST_POSTGRES";

    private static readonly DateTimeOffset Start = new(2026, 11, 14, 12, 0, 0, TimeSpan.Zero);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

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

        // A profiles migration reads users.Users, so migrate in the same order as the app does.
        DbContextOptions<ZagejmiUsersContext> usersOptions = new DbContextOptionsBuilder<ZagejmiUsersContext>()
            .UseNpgsql(connection, npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "users"))
            .Options;
        await using (ZagejmiUsersContext users = new(usersOptions))
        {
            await users.Database.MigrateAsync(Ct);
        }

        this.factory = new TestContextFactory(connection);
        await using ProfilesContext context = this.factory.CreateDbContext();
        await context.Database.MigrateAsync(Ct);
    }

    public async ValueTask DisposeAsync()
    {
        if (this.factory is null)
        {
            return;
        }

        await using ProfilesContext context = this.factory.CreateDbContext();
        await context.Database.EnsureDeletedAsync(Ct);
    }

    [Fact]
    public async Task Upsert_keeps_one_review_per_author_with_its_original_id_and_creation_time()
    {
        (ProfileRepository profiles, PlayerReviewRepository reviews) = this.CreateRepositories();
        PlayerProfile player = await AddPlayerAsync(profiles, "Hráč");
        CustomerProfile jana = await AddCustomerAsync(profiles, "jana");

        PlayerReview first = Review(player, jana, 2, "Nic moc", Start);
        await reviews.UpsertAsync(first, Ct);
        await reviews.UpsertAsync(Review(player, jana, 5, null, Start.AddDays(1)), Ct);

        PlayerReview stored = (await reviews.GetAsync(player.Id, jana.Id, Ct))!;
        Assert.Equal(first.Id, stored.Id);
        Assert.Equal(first.CreatedAt, stored.CreatedAt);
        Assert.Equal(Start.AddDays(1), stored.UpdatedAt);
        Assert.Equal(5, stored.Rating);
        Assert.Null(stored.Comment);
        Assert.Equal(1, (await reviews.GetForPlayerAsync(player.Id, 0, 10, Ct)).TotalCount);
    }

    [Fact]
    public async Task Lists_newest_first_with_authors_and_averages_per_player()
    {
        (ProfileRepository profiles, PlayerReviewRepository reviews) = this.CreateRepositories();
        PlayerProfile anna = await AddPlayerAsync(profiles, "Anna");
        PlayerProfile bob = await AddPlayerAsync(profiles, "Bob");
        PlayerProfile nobody = await AddPlayerAsync(profiles, "Nikdo");
        CustomerProfile jana = await AddCustomerAsync(profiles, "jana");
        CustomerProfile petr = await AddCustomerAsync(profiles, "petr");
        CustomerProfile eva = await AddCustomerAsync(profiles, "eva");

        await reviews.UpsertAsync(Review(anna, jana, 5, "Skvělá", Start), Ct);
        await reviews.UpsertAsync(Review(anna, petr, 4, null, Start.AddHours(1)), Ct);
        await reviews.UpsertAsync(Review(anna, eva, 3, "Ujde to", Start.AddHours(2)), Ct);
        await reviews.UpsertAsync(Review(bob, jana, 1, null, Start), Ct);

        PagedResult<PlayerReviewItem> firstPage = await reviews.GetForPlayerAsync(anna.Id, 0, 2, Ct);
        Assert.Equal(3, firstPage.TotalCount);
        Assert.Equal(["eva", "petr"], firstPage.Items.Select(r => r.AuthorName).ToList());
        Assert.Equal("jana", Assert.Single((await reviews.GetForPlayerAsync(anna.Id, 2, 2, Ct)).Items).AuthorName);

        IReadOnlyDictionary<Guid, RatingSummary> summaries =
            await reviews.GetSummariesAsync([anna.Id, bob.Id, nobody.Id], Ct);
        Assert.Equal(new RatingSummary(4, 3), summaries[anna.Id]);
        Assert.Equal(new RatingSummary(1, 1), summaries[bob.Id]);
        Assert.False(summaries.ContainsKey(nobody.Id));

        Assert.True(await reviews.DeleteAsync(anna.Id, petr.Id, Ct));
        Assert.False(await reviews.DeleteAsync(anna.Id, petr.Id, Ct));
        Assert.Equal(new RatingSummary(4, 2), (await reviews.GetSummariesAsync([anna.Id], Ct))[anna.Id]);
    }

    [Fact]
    public async Task Database_rejects_ratings_outside_one_to_five()
    {
        (ProfileRepository profiles, PlayerReviewRepository reviews) = this.CreateRepositories();
        PlayerProfile player = await AddPlayerAsync(profiles, "Hráč");
        CustomerProfile jana = await AddCustomerAsync(profiles, "jana");

        PostgresException error = await Assert.ThrowsAsync<PostgresException>(
            () => reviews.UpsertAsync(Review(player, jana, 6, null, Start), Ct));

        Assert.Equal("CK_PlayerReviews_Rating", error.ConstraintName);
    }

    [Fact]
    public async Task Reviews_disappear_with_the_player()
    {
        (ProfileRepository profiles, PlayerReviewRepository reviews) = this.CreateRepositories();
        PlayerProfile player = await AddPlayerAsync(profiles, "Hráč");
        CustomerProfile jana = await AddCustomerAsync(profiles, "jana");
        await reviews.UpsertAsync(Review(player, jana, 4, null, Start), Ct);

        await using (ProfilesContext context = this.factory!.CreateDbContext())
        {
            await context.Players.Where(p => p.Id == player.Id).ExecuteDeleteAsync(Ct);
        }

        await using ProfilesContext check = this.factory.CreateDbContext();
        Assert.Equal(0, await check.PlayerReviews.CountAsync(Ct));
    }

    private static PlayerReview Review(PlayerProfile player, CustomerProfile author, int rating, string? comment, DateTimeOffset at) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            PlayerProfileId = player.Id,
            AuthorProfileId = author.Id,
            Rating = rating,
            Comment = comment,
            CreatedAt = at,
            UpdatedAt = at,
        };

    private static async Task<PlayerProfile> AddPlayerAsync(ProfileRepository profiles, string name)
    {
        PlayerProfile player = new(Guid.CreateVersion7(), Guid.CreateVersion7(), name);
        await profiles.AddPlayerAsync(player, Ct);
        return player;
    }

    private static async Task<CustomerProfile> AddCustomerAsync(ProfileRepository profiles, string name)
    {
        CustomerProfile customer = new(Guid.CreateVersion7(), Guid.CreateVersion7(), name);
        await profiles.AddCustomerAsync(customer, Ct);
        return customer;
    }

    private (ProfileRepository Profiles, PlayerReviewRepository Reviews) CreateRepositories()
    {
        Assert.SkipWhen(this.factory is null, $"Set {ConnectionVariable} to run the PostgreSQL review tests.");
        return (new ProfileRepository(this.factory!), new PlayerReviewRepository(this.factory!));
    }

    private sealed class TestContextFactory : IDbContextFactory<ProfilesContext>
    {
        private readonly DbContextOptions<ProfilesContext> options;

        public TestContextFactory(string connection)
        {
            this.options = new DbContextOptionsBuilder<ProfilesContext>()
                .UseNpgsql(connection, npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "profiles"))
                .Options;
        }

        public ProfilesContext CreateDbContext()
        {
            return new ProfilesContext(this.options);
        }
    }
}
