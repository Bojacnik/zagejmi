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
///     Runs the service review queries (including the raw upsert by order) against a real PostgreSQL (set ZAGEJMI_TEST_POSTGRES, see the
///     wallet tests); each run uses its own throw-away database.
/// </summary>
public sealed class ServiceReviewRepositoryTests : IAsyncLifetime
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
    public async Task One_review_per_order_upsert_keeps_id_and_creation_time()
    {
        (ProfileRepository profiles, ServiceReviewRepository reviews) = this.CreateRepositories();
        PlayerProfile player = await AddPlayerAsync(profiles, "Hráč");
        CustomerProfile jana = await AddCustomerAsync(profiles, "jana");
        Guid orderId = Guid.CreateVersion7();

        ServiceReview first = Review(orderId, Guid.CreateVersion7(), player, jana, 2, "Nic moc", Start);
        await reviews.UpsertAsync(first, Ct);
        await reviews.UpsertAsync(Review(orderId, first.ServiceId, player, jana, 5, null, Start.AddDays(1)), Ct);

        ServiceReview stored = (await reviews.GetByOrderAsync(orderId, Ct))!;
        Assert.Equal(first.Id, stored.Id);
        Assert.Equal(Start, stored.CreatedAt);
        Assert.Equal(5, stored.Rating);
        Assert.Null(stored.Comment);
        Assert.Equal(1, (await reviews.GetForPlayerAsync(player.Id, 0, 10, Ct)).TotalCount);
    }

    [Fact]
    public async Task Lists_with_service_names_and_averages_per_service_and_knows_reviewed_orders()
    {
        (ProfileRepository profiles, ServiceReviewRepository reviews) = this.CreateRepositories();
        PlayerProfile player = await AddPlayerAsync(profiles, "Hráč");
        CustomerProfile jana = await AddCustomerAsync(profiles, "jana");
        Guid duo = Guid.CreateVersion7();
        Guid coaching = Guid.CreateVersion7();
        Guid[] orders = [Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7()];

        // The same customer reviews three orders: regulars can review every completed order.
        await reviews.UpsertAsync(Review(orders[0], duo, player, jana, 5, "Super", Start), Ct);
        await reviews.UpsertAsync(Review(orders[1], duo, player, jana, 3, null, Start.AddHours(1)), Ct);
        await reviews.UpsertAsync(Review(orders[2], coaching, player, jana, 4, null, Start.AddHours(2), "Koučink"), Ct);

        PagedResult<ServiceReviewItem> page = await reviews.GetForPlayerAsync(player.Id, 0, 2, Ct);
        Assert.Equal(3, page.TotalCount);
        Assert.Equal(["Koučink", "Valorant duo"], page.Items.Select(r => r.ServiceName).ToList());
        Assert.All(page.Items, r => Assert.Equal("jana", r.AuthorName));

        IReadOnlyDictionary<Guid, RatingSummary> summaries = await reviews.GetSummariesAsync([duo, coaching, Guid.CreateVersion7()], Ct);
        Assert.Equal(new RatingSummary(4, 2), summaries[duo]);
        Assert.Equal(new RatingSummary(4, 1), summaries[coaching]);
        Assert.Equal(2, summaries.Count);

        Guid unreviewed = Guid.CreateVersion7();
        Assert.Equal(
            new HashSet<Guid> { orders[0], orders[2] },
            (await reviews.GetReviewedOrderIdsAsync([orders[0], orders[2], unreviewed], Ct)).ToHashSet());

        Assert.True(await reviews.DeleteByOrderAsync(orders[1], Ct));
        Assert.Equal(new RatingSummary(5, 1), (await reviews.GetSummariesAsync([duo], Ct))[duo]);
    }

    [Fact]
    public async Task Review_outlives_a_removed_service_but_not_the_player()
    {
        (ProfileRepository profiles, ServiceReviewRepository reviews) = this.CreateRepositories();
        PlayerProfile player = await AddPlayerAsync(profiles, "Hráč");
        CustomerProfile jana = await AddCustomerAsync(profiles, "jana");
        Guid orderId = Guid.CreateVersion7();
        await reviews.UpsertAsync(Review(orderId, Guid.CreateVersion7(), player, jana, 4, null, Start), Ct);

        await using (ProfilesContext context = this.factory!.CreateDbContext())
        {
            Assert.Equal(0, await context.PlayerServices.CountAsync(Ct)); // never existed: no foreign key to services
            await context.Players.Where(p => p.Id == player.Id).ExecuteDeleteAsync(Ct);
        }

        Assert.Null(await reviews.GetByOrderAsync(orderId, Ct));
    }

    private static ServiceReview Review(
        Guid orderId,
        Guid serviceId,
        PlayerProfile player,
        CustomerProfile author,
        int rating,
        string? comment,
        DateTimeOffset at,
        string serviceName = "Valorant duo") =>
        new()
        {
            Id = Guid.CreateVersion7(),
            OrderId = orderId,
            ServiceId = serviceId,
            ServiceName = serviceName,
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

    private (ProfileRepository Profiles, ServiceReviewRepository Reviews) CreateRepositories()
    {
        Assert.SkipWhen(this.factory is null, $"Set {ConnectionVariable} to run the PostgreSQL service review tests.");
        return (new ProfileRepository(this.factory!), new ServiceReviewRepository(this.factory!));
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
