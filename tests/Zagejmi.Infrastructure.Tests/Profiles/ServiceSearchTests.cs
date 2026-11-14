using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Xunit;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Profiles;
using Zagejmi.Application.Profiles.SearchServices;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Games;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Infrastructure.Auth.Users.Ctx;
using Zagejmi.Infrastructure.Auth.Users.Records;
using Zagejmi.Infrastructure.Profiles.Ctx;
using Zagejmi.Infrastructure.Profiles.Repositories;

namespace Zagejmi.Infrastructure.Tests.Profiles;

/// <summary>
///     Runs the Služby query against a real PostgreSQL (set ZAGEJMI_TEST_POSTGRES, see the wallet tests); each run uses
///     its own throw-away database.
/// </summary>
public sealed class ServiceSearchTests : IAsyncLifetime
{
    private const string ConnectionVariable = "ZAGEJMI_TEST_POSTGRES";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private readonly string? serverConnection = Environment.GetEnvironmentVariable(ConnectionVariable);
    private readonly string databaseName = $"zagejmi_test_{Guid.NewGuid():N}";
    private TestContextFactory? factory;
    private DbContextOptions<ZagejmiUsersContext>? usersOptions;

    public async ValueTask InitializeAsync()
    {
        if (this.serverConnection is null)
        {
            return;
        }

        string connection = new NpgsqlConnectionStringBuilder(this.serverConnection) { Database = this.databaseName }
            .ConnectionString;

        // A profiles migration reads users.Users, so migrate in the same order as the app does.
        this.usersOptions = new DbContextOptionsBuilder<ZagejmiUsersContext>()
            .UseNpgsql(connection, npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "users"))
            .Options;
        await using (ZagejmiUsersContext users = new(this.usersOptions))
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
    public async Task Filters_by_text_game_minimum_rank_and_play_style()
    {
        ProfileRepository repository = this.CreateRepository();
        PlayerProfile anna = await AddPlayerAsync(repository, "Anna");
        PlayerProfile bob = await AddPlayerAsync(repository, "Bob_100%");

        await AddServiceAsync(repository, anna, "Valorant duo", "valorant", "gold", PlayStyles.Competitive, 1);
        await AddServiceAsync(repository, anna, "Valorant pohoda", "valorant", null, PlayStyles.Casual, 2);
        await AddServiceAsync(repository, bob, "Radiant koučink", "valorant", "radiant", PlayStyles.Both, 3);
        await AddServiceAsync(repository, bob, "LoL duo", "league-of-legends", "gold", PlayStyles.Competitive, 4);
        await AddServiceAsync(repository, bob, "Pokec", null, null, PlayStyles.None, 5);
        await AddServiceAsync(repository, bob, "Nedostupná", "valorant", "radiant", PlayStyles.Both, 6, isAvailable: false);

        Assert.Equal(
            ["Pokec", "LoL duo", "Radiant koučink", "Valorant pohoda", "Valorant duo"],
            await NamesAsync(repository, new ServiceSearchFilter(null, null, null, PlayStyles.None)));

        Assert.Equal(
            ["Radiant koučink", "Valorant pohoda", "Valorant duo"],
            await NamesAsync(repository, new ServiceSearchFilter(null, "valorant", null, PlayStyles.None)));

        Assert.Equal(
            ["Radiant koučink"],
            await NamesAsync(repository, new ServiceSearchFilter(null, "valorant", ["immortal", "radiant"], PlayStyles.None)));

        Assert.Equal(
            ["Radiant koučink", "Valorant pohoda"],
            await NamesAsync(repository, new ServiceSearchFilter(null, "valorant", null, PlayStyles.Casual)));

        // Text matches the service name or the player name; LIKE wildcards in the input are literal.
        Assert.Equal(["Radiant koučink"], await NamesAsync(repository, new ServiceSearchFilter("KOUČ", null, null, PlayStyles.None)));
        Assert.Equal(3, (await NamesAsync(repository, new ServiceSearchFilter("100%", null, null, PlayStyles.None))).Count);
        Assert.Empty(await NamesAsync(repository, new ServiceSearchFilter("B_b", null, null, PlayStyles.None)));

        ServiceListItem item = (await repository.SearchServicesAsync(
            new ServiceSearchFilter("Radiant", null, null, PlayStyles.None), 0, 10, Ct)).Items.Single();
        Assert.Equal(bob.Id, item.PlayerProfileId);
        Assert.Equal("Bob_100%", item.PlayerDisplayName);
    }

    [Fact]
    public async Task Splits_players_and_services_by_gender()
    {
        ProfileRepository repository = this.CreateRepository();
        PlayerProfile anna = await this.AddPlayerWithGenderAsync(repository, "Anna", Gender.Female);
        PlayerProfile bob = await this.AddPlayerWithGenderAsync(repository, "Bob", Gender.Male);
        PlayerProfile kim = await this.AddPlayerWithGenderAsync(repository, "Kim", Gender.Other);
        PlayerProfile lee = await this.AddPlayerWithGenderAsync(repository, "Lee", Gender.Unknown);

        await AddServiceAsync(repository, anna, "Annina služba", "valorant", "gold", PlayStyles.Competitive, 1);
        await AddServiceAsync(repository, bob, "Bobova služba", "valorant", "gold", PlayStyles.Competitive, 2);
        await AddServiceAsync(repository, kim, "Kimova služba", "valorant", "gold", PlayStyles.Casual, 3);
        await AddServiceAsync(repository, lee, "Leeho služba", "valorant", "gold", PlayStyles.Competitive, 4);

        Assert.Equal(["Anna"], await PlayerNamesAsync(repository, GenderGroup.Women));
        Assert.Equal(["Bob"], await PlayerNamesAsync(repository, GenderGroup.Men));
        Assert.Equal(["Lee", "Kim"], await PlayerNamesAsync(repository, GenderGroup.Others));
        Assert.Equal(["Lee", "Kim", "Bob", "Anna"], await PlayerNamesAsync(repository, null));

        // The total counts every match, not just the requested page.
        PagedResult<PlayerProfile> firstOfAll = await repository.SearchPlayersAsync(null, null, 0, 1, Ct);
        Assert.Single(firstOfAll.Items);
        Assert.Equal(4, firstOfAll.TotalCount);
        Assert.Equal(2, (await repository.SearchServicesAsync(new(null, null, null, PlayStyles.None, GenderGroup.Others), 0, 1, Ct)).TotalCount);

        Assert.Equal(["Annina služba"], await NamesAsync(repository, new(null, null, null, PlayStyles.None, GenderGroup.Women)));
        Assert.Equal(["Bobova služba"], await NamesAsync(repository, new(null, null, null, PlayStyles.None, GenderGroup.Men)));

        // Other filters still apply inside a group.
        Assert.Equal(["Leeho služba"], await NamesAsync(repository, new(null, "valorant", null, PlayStyles.Competitive, GenderGroup.Others)));
    }

    [Fact]
    public async Task Prefers_profile_picture_then_banner_then_service_images_then_newest()
    {
        ProfileRepository repository = this.CreateRepository();

        // Created oldest to newest, so without the weights the order would be reversed.
        PlayerProfile pictureOnly = await AddPlayerAsync(repository, "PictureOnly", picture: true);
        PlayerProfile bannerAndImage = await AddPlayerAsync(repository, "BannerAndImage", banner: true);
        PlayerProfile bannerOnly = await AddPlayerAsync(repository, "BannerOnly", banner: true);
        PlayerProfile imageOnly = await AddPlayerAsync(repository, "ImageOnly");
        PlayerProfile hiddenImage = await AddPlayerAsync(repository, "HiddenImage");
        PlayerProfile bare = await AddPlayerAsync(repository, "Bare");
        PlayerProfile full = await AddPlayerAsync(repository, "Full", picture: true, banner: true);

        await AddServiceAsync(repository, pictureOnly, "S PictureOnly", null, null, PlayStyles.None, 1);
        await AddServiceAsync(repository, bannerAndImage, "S BannerAndImage", null, null, PlayStyles.None, 2, image: true);
        await AddServiceAsync(repository, bannerOnly, "S BannerOnly", null, null, PlayStyles.None, 3);
        await AddServiceAsync(repository, imageOnly, "S ImageOnly", null, null, PlayStyles.None, 4, image: true);
        await AddServiceAsync(repository, hiddenImage, "S HiddenImage", null, null, PlayStyles.None, 5, image: true, isAvailable: false);
        await AddServiceAsync(repository, bare, "S Bare", null, null, PlayStyles.None, 6);
        await AddServiceAsync(repository, full, "S Full", null, null, PlayStyles.None, 7, image: true);

        // Picture 4 > banner 2 > service image 1: a picture alone beats banner + image, a banner alone beats an image.
        // An image on an unavailable service does not count; Bare and HiddenImage tie at 0, so the newer comes first.
        Assert.Equal(
            ["Full", "PictureOnly", "BannerAndImage", "BannerOnly", "ImageOnly", "Bare", "HiddenImage"],
            (await repository.SearchPlayersAsync(null, null, 0, 10, Ct)).Items.Select(p => p.DisplayName).ToList());

        Assert.Equal(
            ["S Full", "S PictureOnly", "S BannerAndImage", "S BannerOnly", "S ImageOnly", "S Bare"],
            await NamesAsync(repository, new ServiceSearchFilter(null, null, null, PlayStyles.None)));
    }

    private static async Task<IReadOnlyList<string>> PlayerNamesAsync(ProfileRepository repository, GenderGroup? group)
    {
        return (await repository.SearchPlayersAsync(null, group, 0, 50, Ct)).Items.Select(p => p.DisplayName).ToList();
    }

    private static async Task<IReadOnlyList<string>> NamesAsync(ProfileRepository repository, ServiceSearchFilter filter)
    {
        return (await repository.SearchServicesAsync(filter, 0, 50, Ct)).Items.Select(s => s.Name).ToList();
    }

    private static async Task<PlayerProfile> AddPlayerAsync(
        ProfileRepository repository,
        string name,
        bool picture = false,
        bool banner = false)
    {
        PlayerProfile player = new(Guid.CreateVersion7(), Guid.CreateVersion7(), name)
        {
            CardProfilePicture = picture ? Guid.CreateVersion7() : null,
            BannerPicture = banner ? Guid.CreateVersion7() : null,
        };
        await repository.AddPlayerAsync(player, Ct);

        // Keeps CreatedAt strictly increasing for the "newest first" tie-break.
        await Task.Delay(5, Ct);
        return player;
    }

    private static Task AddServiceAsync(
        ProfileRepository repository,
        PlayerProfile player,
        string name,
        string? game,
        string? rank,
        PlayStyles styles,
        int minutesAfter,
        bool isAvailable = true,
        bool image = false)
    {
        return repository.AddPlayerServiceAsync(
            new PlayerService
            {
                Id = Guid.CreateVersion7(),
                PlayerProfileId = player.Id,
                Name = name,
                GameKey = game,
                RankKey = rank,
                PlayStyles = styles,
                IsAvailable = isAvailable,
                ImageId = image ? Guid.CreateVersion7() : null,
                CreatedAt = new DateTimeOffset(2026, 11, 13, 12, minutesAfter, 0, TimeSpan.Zero),
            },
            Ct);
    }

    private async Task<PlayerProfile> AddPlayerWithGenderAsync(ProfileRepository repository, string name, Gender gender)
    {
        Guid userId = Guid.CreateVersion7();
        await using (ZagejmiUsersContext users = new(this.usersOptions!))
        {
            users.Users.Add(new UserRecord
            {
                Id = userId,
                UserName = name,
                Email = $"{name}@example.test",
                PendingEmailVerificationTokenHash = null,
                PasswordHash = "x",
                Gender = gender,
            });
            await users.SaveChangesAsync(Ct);
        }

        // Created in order, so "newest first" lists the later players first.
        PlayerProfile player = new(Guid.CreateVersion7(), userId, name);
        await repository.AddPlayerAsync(player, Ct);
        await Task.Delay(5, Ct);
        return player;
    }

    private ProfileRepository CreateRepository()
    {
        Assert.SkipWhen(this.factory is null, $"Set {ConnectionVariable} to run the PostgreSQL service search tests.");
        return new ProfileRepository(this.factory!);
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
