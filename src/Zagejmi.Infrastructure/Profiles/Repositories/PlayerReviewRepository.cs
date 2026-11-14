using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Profiles.Reviews;
using Zagejmi.Application.Profiles.Reviews.Abstractions;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Infrastructure.Profiles.Ctx;
using Zagejmi.Infrastructure.Profiles.Records;

namespace Zagejmi.Infrastructure.Profiles.Repositories;

/// <summary>
///     Stores player reviews in profiles.PlayerReviews.
/// </summary>
public sealed class PlayerReviewRepository : IPlayerReviewRepository
{
    private readonly IDbContextFactory<ProfilesContext> contextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PlayerReviewRepository" /> class.
    /// </summary>
    /// <param name="contextFactory">Creates a short-lived database context per operation.</param>
    public PlayerReviewRepository(IDbContextFactory<ProfilesContext> contextFactory)
    {
        this.contextFactory = contextFactory;
    }

    /// <inheritdoc />
    public async Task<PlayerReview?> GetAsync(
        Guid playerProfileId,
        Guid authorProfileId,
        CancellationToken cancellationToken = default)
    {
        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);
        PlayerReviewRecord? record = await context.PlayerReviews
            .AsNoTracking()
            .SingleOrDefaultAsync(
                r => r.PlayerProfileId == playerProfileId && r.AuthorProfileId == authorProfileId,
                cancellationToken);

        return record is null
            ? null
            : new PlayerReview
            {
                Id = record.Id,
                PlayerProfileId = record.PlayerProfileId,
                AuthorProfileId = record.AuthorProfileId,
                Rating = record.Rating,
                Comment = record.Comment,
                CreatedAt = record.CreatedAt,
                UpdatedAt = record.UpdatedAt,
            };
    }

    /// <inheritdoc />
    public async Task UpsertAsync(PlayerReview review, CancellationToken cancellationToken = default)
    {
        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        // One statement, so two quick submits by the same customer cannot race into a duplicate-key error. On conflict
        // the original Id and CreatedAt stay.
        await context.Database.ExecuteSqlAsync(
            $"""
             INSERT INTO profiles."PlayerReviews"
                 ("Id", "PlayerProfileId", "AuthorProfileId", "Rating", "Comment", "CreatedAt", "UpdatedAt")
             VALUES ({review.Id}, {review.PlayerProfileId}, {review.AuthorProfileId}, {review.Rating},
                     {review.Comment}, {review.CreatedAt}, {review.UpdatedAt})
             ON CONFLICT ("PlayerProfileId", "AuthorProfileId") DO UPDATE
             SET "Rating" = EXCLUDED."Rating", "Comment" = EXCLUDED."Comment", "UpdatedAt" = EXCLUDED."UpdatedAt"
             """,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(
        Guid playerProfileId,
        Guid authorProfileId,
        CancellationToken cancellationToken = default)
    {
        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);
        int deleted = await context.PlayerReviews
            .Where(r => r.PlayerProfileId == playerProfileId && r.AuthorProfileId == authorProfileId)
            .ExecuteDeleteAsync(cancellationToken);

        return deleted > 0;
    }

    /// <inheritdoc />
    public async Task<PagedResult<PlayerReviewItem>> GetForPlayerAsync(
        Guid playerProfileId,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);
        IQueryable<PlayerReviewRecord> reviews = context.PlayerReviews
            .AsNoTracking()
            .Where(r => r.PlayerProfileId == playerProfileId);

        int total = await reviews.CountAsync(cancellationToken);
        List<PlayerReviewItem> items = await reviews
            .Join(
                context.Customers,
                r => r.AuthorProfileId,
                c => c.Id,
                (r, c) => new { Review = r, Author = c })
            .OrderByDescending(x => x.Review.CreatedAt)
            .ThenBy(x => x.Review.Id)
            .Skip(skip)
            .Take(take)
            .Select(x => new PlayerReviewItem(
                x.Author.Id,
                x.Author.DisplayName,
                x.Author.ProfilePictureId,
                x.Review.Rating,
                x.Review.Comment,
                x.Review.CreatedAt,
                x.Review.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<PlayerReviewItem>(items, total);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, RatingSummary>> GetSummariesAsync(
        IReadOnlyCollection<Guid> playerProfileIds,
        CancellationToken cancellationToken = default)
    {
        if (playerProfileIds.Count == 0)
        {
            return new Dictionary<Guid, RatingSummary>();
        }

        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);
        var rows = await context.PlayerReviews
            .AsNoTracking()
            .Where(r => playerProfileIds.Contains(r.PlayerProfileId))
            .GroupBy(r => r.PlayerProfileId)
            .Select(g => new { PlayerProfileId = g.Key, Average = g.Average(r => (double)r.Rating), Count = g.Count() })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(r => r.PlayerProfileId, r => new RatingSummary(r.Average, r.Count));
    }
}
