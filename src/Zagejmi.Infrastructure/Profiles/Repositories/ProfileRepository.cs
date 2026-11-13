using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Profiles;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Application.Profiles.SearchServices;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Games;
using Zagejmi.Domain.Profiles.Customers;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Infrastructure.Profiles.Ctx;
using Zagejmi.Infrastructure.Profiles.Records;

namespace Zagejmi.Infrastructure.Profiles.Repositories;

/// <summary>
///     EF Core implementation of <see cref="IProfileRepository" />.
/// </summary>
public sealed class ProfileRepository : IProfileRepository
{
    private readonly IDbContextFactory<ProfilesContext> contextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProfileRepository" /> class.
    /// </summary>
    /// <param name="contextFactory">Creates a short-lived database context per operation, so concurrent calls on one Blazor circuit never share a DbContext.</param>
    public ProfileRepository(IDbContextFactory<ProfilesContext> contextFactory)
    {
        this.contextFactory = contextFactory;
    }

    /// <inheritdoc />
    public async Task<CustomerProfile?> GetCustomerByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        CustomerProfileRecord? record = await context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        return record is null
            ? null
            : new CustomerProfile(record.Id, record.UserId, record.DisplayName)
            {
                CreatedAt = record.CreatedAt,
                ProfilePicture = record.ProfilePictureId,
            };
    }

    /// <inheritdoc />
    public async Task<PlayerProfile?> GetPlayerByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        PlayerProfileRecord? record = await context.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        return record is null ? null : ToDomain(record);
    }

    /// <inheritdoc />
    public async Task<PlayerProfile?> GetPlayerByIdAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        PlayerProfileRecord? record = await context.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == profileId, cancellationToken);

        return record is null ? null : ToDomain(record);
    }

    /// <inheritdoc />
    public async Task<PagedResult<PlayerProfile>> SearchPlayersAsync(
        string? search,
        GenderGroup? gender,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        IQueryable<PlayerProfileRecord> query = context.Players.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            string pattern = $"%{EscapeLikePattern(search.Trim())}%";
            query = query.Where(x => EF.Functions.ILike(x.DisplayName, pattern, "\\"));
        }

        if (gender is { } group)
        {
            IQueryable<ProfileOwnerRecord> owners = context.ProfileOwners.Where(InGroup(group));
            query = query.Where(x => owners.Any(o => o.Id == x.UserId));
        }

        // Complete profiles first (RANK-001, weights in ProfileCompleteness): profile picture, then banner, then an
        // available service with an image; ties go to the newest.
        IQueryable<PlayerServiceRecord> servicesWithImage = context.PlayerServices.Where(s => s.IsAvailable && s.ImageId != null);
        int total = await query.CountAsync(cancellationToken);
        List<PlayerProfileRecord> records = await query
            .OrderByDescending(x =>
                (x.CardProfilePictureId != null ? ProfileCompleteness.ProfilePicture : 0)
                + (x.BannerPictureId != null ? ProfileCompleteness.Banner : 0)
                + (servicesWithImage.Any(s => s.PlayerProfileId == x.Id) ? ProfileCompleteness.ServiceImage : 0))
            .ThenByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new PagedResult<PlayerProfile>(records.Select(ToDomain).ToList(), total);
    }

    /// <inheritdoc />
    public async Task<PagedResult<ServiceListItem>> SearchServicesAsync(
        ServiceSearchFilter filter,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.PlayerServices
            .AsNoTracking()
            .Where(s => s.IsAvailable)
            .Join(context.Players, s => s.PlayerProfileId, p => p.Id, (s, p) => new { Service = s, Player = p });

        if (filter.Search is { } search)
        {
            string pattern = $"%{EscapeLikePattern(search)}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.Service.Name, pattern, "\\")
                || EF.Functions.ILike(x.Player.DisplayName, pattern, "\\"));
        }

        if (filter.Gender is { } group)
        {
            IQueryable<ProfileOwnerRecord> owners = context.ProfileOwners.Where(InGroup(group));
            query = query.Where(x => owners.Any(o => o.Id == x.Player.UserId));
        }

        if (filter.GameKey is { } gameKey)
        {
            query = query.Where(x => x.Service.GameKey == gameKey);
        }

        if (filter.RankKeys is { } rankKeys)
        {
            query = query.Where(x => x.Service.RankKey != null && rankKeys.Contains(x.Service.RankKey));
        }

        if (filter.PlayStyles != PlayStyles.None)
        {
            PlayStyles wanted = filter.PlayStyles;
            query = query.Where(x => (x.Service.PlayStyles & wanted) != PlayStyles.None);
        }

        int total = await query.CountAsync(cancellationToken);
        List<ServiceListItem> items = await query
            // Complete listings first (RANK-001, weights in ProfileCompleteness): the player's profile picture, then
            // their banner, then the service image; ties go to the newest.
            .OrderByDescending(x =>
                (x.Player.CardProfilePictureId != null ? ProfileCompleteness.ProfilePicture : 0)
                + (x.Player.BannerPictureId != null ? ProfileCompleteness.Banner : 0)
                + (x.Service.ImageId != null ? ProfileCompleteness.ServiceImage : 0))
            .ThenByDescending(x => x.Service.CreatedAt)
            .ThenBy(x => x.Service.Id)
            .Skip(skip)
            .Take(take)
            .Select(x => new ServiceListItem(
                x.Service.Id,
                x.Player.Id,
                x.Player.DisplayName,
                x.Player.CardProfilePictureId,
                x.Service.Name,
                x.Service.Description,
                x.Service.Price,
                x.Service.PriceUnit,
                x.Service.Category,
                x.Service.GameKey,
                x.Service.RankKey,
                x.Service.PlayStyles,
                x.Service.ImageId,
                x.Service.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<ServiceListItem>(items, total);
    }

    /// <inheritdoc />
    public async Task<bool> IsDisplayNameTakenAsync(string displayName, CancellationToken cancellationToken = default)
    {
        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Customers.AnyAsync(x => x.DisplayName == displayName, cancellationToken)
               || await context.Players.AnyAsync(x => x.DisplayName == displayName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddCustomerAsync(CustomerProfile profile, CancellationToken cancellationToken = default)
    {
        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        context.Customers.Add(new CustomerProfileRecord
        {
            Id = profile.Id,
            UserId = profile.UserId,
            CreatedAt = profile.CreatedAt,
            DisplayName = profile.DisplayName,
            ProfilePictureId = profile.ProfilePicture,
        });

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddPlayerAsync(PlayerProfile profile, CancellationToken cancellationToken = default)
    {
        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        context.Players.Add(new PlayerProfileRecord
        {
            Id = profile.Id,
            UserId = profile.UserId,
            CreatedAt = profile.CreatedAt,
            DisplayName = profile.DisplayName,
            CardProfilePictureId = profile.CardProfilePicture,
            BannerPictureId = profile.BannerPicture,
            BackgroundServicesPictureId = profile.BackgroundServicesPicture,
            Description = profile.Description,
        });

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateCustomerAsync(CustomerProfile profile, CancellationToken cancellationToken = default)
    {
        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        int updated = await context.Customers
            .Where(x => x.Id == profile.Id)
            .ExecuteUpdateAsync(
                x => x
                    .SetProperty(r => r.DisplayName, profile.DisplayName)
                    .SetProperty(r => r.ProfilePictureId, profile.ProfilePicture),
                cancellationToken);

        if (updated == 0)
        {
            throw new InvalidOperationException($"Customer profile '{profile.Id}' does not exist.");
        }
    }

    /// <inheritdoc />
    public async Task UpdatePlayerAsync(PlayerProfile profile, CancellationToken cancellationToken = default)
    {
        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        int updated = await context.Players
            .Where(x => x.Id == profile.Id)
            .ExecuteUpdateAsync(
                x => x
                    .SetProperty(r => r.DisplayName, profile.DisplayName)
                    .SetProperty(r => r.Description, profile.Description)
                    .SetProperty(r => r.CardProfilePictureId, profile.CardProfilePicture)
                    .SetProperty(r => r.BannerPictureId, profile.BannerPicture)
                    .SetProperty(r => r.BackgroundServicesPictureId, profile.BackgroundServicesPicture),
                cancellationToken);

        if (updated == 0)
        {
            throw new InvalidOperationException($"Player profile '{profile.Id}' does not exist.");
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlayerService>> GetPlayerServicesAsync(
        Guid playerProfileId,
        CancellationToken cancellationToken = default)
    {
        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        List<PlayerServiceRecord> records = await context.PlayerServices
            .AsNoTracking()
            .Where(x => x.PlayerProfileId == playerProfileId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return records.Select(ToDomain).ToList();
    }

    /// <inheritdoc />
    public async Task<int> CountPlayerServicesAsync(Guid playerProfileId, CancellationToken cancellationToken = default)
    {
        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.PlayerServices.CountAsync(x => x.PlayerProfileId == playerProfileId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddPlayerServiceAsync(PlayerService service, CancellationToken cancellationToken = default)
    {
        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        context.PlayerServices.Add(new PlayerServiceRecord
        {
            Id = service.Id,
            PlayerProfileId = service.PlayerProfileId,
            Name = service.Name,
            Description = service.Description,
            Category = service.Category,
            GameKey = service.GameKey,
            RankKey = service.RankKey,
            PlayStyles = service.PlayStyles,
            Price = service.Price,
            PriceUnit = service.PriceUnit,
            PromoTag = service.PromoTag,
            ImageId = service.ImageId,
            IsAvailable = service.IsAvailable,
            CreatedAt = service.CreatedAt,
        });

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlayerService?> GetPlayerServiceAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        PlayerServiceRecord? record = await context.PlayerServices
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == serviceId, cancellationToken);

        return record is null ? null : ToDomain(record);
    }

    /// <inheritdoc />
    public async Task UpdatePlayerServiceAsync(PlayerService service, CancellationToken cancellationToken = default)
    {
        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        int updated = await context.PlayerServices
            .Where(x => x.Id == service.Id)
            .ExecuteUpdateAsync(
                x => x
                    .SetProperty(r => r.Name, service.Name)
                    .SetProperty(r => r.Description, service.Description)
                    .SetProperty(r => r.Category, service.Category)
                    .SetProperty(r => r.GameKey, service.GameKey)
                    .SetProperty(r => r.RankKey, service.RankKey)
                    .SetProperty(r => r.PlayStyles, service.PlayStyles)
                    .SetProperty(r => r.Price, service.Price)
                    .SetProperty(r => r.PriceUnit, service.PriceUnit)
                    .SetProperty(r => r.PromoTag, service.PromoTag)
                    .SetProperty(r => r.ImageId, service.ImageId)
                    .SetProperty(r => r.IsAvailable, service.IsAvailable),
                cancellationToken);

        if (updated == 0)
        {
            throw new InvalidOperationException($"Player service '{service.Id}' does not exist.");
        }
    }

    /// <inheritdoc />
    public async Task DeletePlayerServiceAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        await context.PlayerServices
            .Where(x => x.Id == serviceId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private static PlayerService ToDomain(PlayerServiceRecord record)
    {
        return new PlayerService
        {
            Id = record.Id,
            PlayerProfileId = record.PlayerProfileId,
            Name = record.Name,
            Description = record.Description,
            Category = record.Category,
            GameKey = record.GameKey,
            RankKey = record.RankKey,
            PlayStyles = record.PlayStyles,
            Price = record.Price,
            PriceUnit = record.PriceUnit,
            PromoTag = record.PromoTag,
            ImageId = record.ImageId,
            IsAvailable = record.IsAvailable,
            CreatedAt = record.CreatedAt,
        };
    }

    private static PlayerProfile ToDomain(PlayerProfileRecord record)
    {
        return new PlayerProfile(record.Id, record.UserId, record.DisplayName)
        {
            CreatedAt = record.CreatedAt,
            CardProfilePicture = record.CardProfilePictureId,
            BannerPicture = record.BannerPictureId,
            BackgroundServicesPicture = record.BackgroundServicesPictureId,
            Description = record.Description,
        };
    }

    /// <summary>
    ///     Escapes LIKE wildcards so user input such as "50%" is matched literally.
    /// </summary>
    /// <summary>
    ///     Which genders belong to a group of the default split; "Jiné" and "Nechci uvádět" share the last group.
    /// </summary>
    private static Expression<Func<ProfileOwnerRecord, bool>> InGroup(GenderGroup group)
    {
        return group switch
        {
            GenderGroup.Women => o => o.Gender == Gender.Female,
            GenderGroup.Men => o => o.Gender == Gender.Male,
            _ => o => o.Gender != Gender.Female && o.Gender != Gender.Male,
        };
    }

    private static string EscapeLikePattern(string value)
    {
        return value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
    }
}
