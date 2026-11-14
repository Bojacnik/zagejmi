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
///     Stores reviews of completed orders in profiles.ServiceReviews.
/// </summary>
public sealed class ServiceReviewRepository : IServiceReviewRepository
{
    private readonly IDbContextFactory<ProfilesContext> contextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ServiceReviewRepository" /> class.
    /// </summary>
    /// <param name="contextFactory">Creates a short-lived database context per operation.</param>
    public ServiceReviewRepository(IDbContextFactory<ProfilesContext> contextFactory)
    {
        this.contextFactory = contextFactory;
    }

    /// <inheritdoc />
    public async Task<ServiceReview?> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);
        ServiceReviewRecord? r = await context.ServiceReviews
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.OrderId == orderId, cancellationToken);
        return r is null
            ? null
            : new ServiceReview
            {
                Id = r.Id,
                OrderId = r.OrderId,
                ServiceId = r.ServiceId,
                ServiceName = r.ServiceName,
                PlayerProfileId = r.PlayerProfileId,
                AuthorProfileId = r.AuthorProfileId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
            };
    }

    /// <inheritdoc />
    public async Task UpsertAsync(ServiceReview review, CancellationToken cancellationToken = default)
    {
        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        // One statement, so a double submit cannot race into a duplicate-key error; the original Id and CreatedAt stay.
        await context.Database.ExecuteSqlAsync(
            $"""
             INSERT INTO profiles."ServiceReviews"
                 ("Id", "OrderId", "ServiceId", "ServiceName", "PlayerProfileId", "AuthorProfileId", "Rating", "Comment",
                  "CreatedAt", "UpdatedAt")
             VALUES ({review.Id}, {review.OrderId}, {review.ServiceId}, {review.ServiceName}, {review.PlayerProfileId},
                     {review.AuthorProfileId}, {review.Rating}, {review.Comment}, {review.CreatedAt}, {review.UpdatedAt})
             ON CONFLICT ("OrderId") DO UPDATE
             SET "Rating" = EXCLUDED."Rating", "Comment" = EXCLUDED."Comment", "UpdatedAt" = EXCLUDED."UpdatedAt"
             """,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteByOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.ServiceReviews.Where(r => r.OrderId == orderId).ExecuteDeleteAsync(cancellationToken) > 0;
    }

    /// <inheritdoc />
    public async Task<PagedResult<ServiceReviewItem>> GetForPlayerAsync(
        Guid playerProfileId,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);
        IQueryable<ServiceReviewRecord> reviews = context.ServiceReviews
            .AsNoTracking()
            .Where(r => r.PlayerProfileId == playerProfileId);

        int total = await reviews.CountAsync(cancellationToken);
        List<ServiceReviewItem> items = await reviews
            .Join(context.Customers, r => r.AuthorProfileId, c => c.Id, (r, c) => new { Review = r, Author = c })
            .OrderByDescending(x => x.Review.CreatedAt)
            .ThenBy(x => x.Review.Id)
            .Skip(skip)
            .Take(take)
            .Select(x => new ServiceReviewItem(
                x.Review.OrderId,
                x.Review.ServiceId,
                x.Review.ServiceName,
                x.Author.Id,
                x.Author.DisplayName,
                x.Author.ProfilePictureId,
                x.Review.Rating,
                x.Review.Comment,
                x.Review.CreatedAt,
                x.Review.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<ServiceReviewItem>(items, total);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, RatingSummary>> GetSummariesAsync(
        IReadOnlyCollection<Guid> serviceIds,
        CancellationToken cancellationToken = default)
    {
        if (serviceIds.Count == 0)
        {
            return new Dictionary<Guid, RatingSummary>();
        }

        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);
        var rows = await context.ServiceReviews
            .AsNoTracking()
            .Where(r => serviceIds.Contains(r.ServiceId))
            .GroupBy(r => r.ServiceId)
            .Select(g => new { ServiceId = g.Key, Average = g.Average(r => (double)r.Rating), Count = g.Count() })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(r => r.ServiceId, r => new RatingSummary(r.Average, r.Count));
    }

    /// <inheritdoc />
    public async Task<IReadOnlySet<Guid>> GetReviewedOrderIdsAsync(
        IReadOnlyCollection<Guid> orderIds,
        CancellationToken cancellationToken = default)
    {
        if (orderIds.Count == 0)
        {
            return new HashSet<Guid>();
        }

        await using ProfilesContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);
        List<Guid> reviewed = await context.ServiceReviews
            .AsNoTracking()
            .Where(r => orderIds.Contains(r.OrderId))
            .Select(r => r.OrderId)
            .ToListAsync(cancellationToken);
        return reviewed.ToHashSet();
    }
}
