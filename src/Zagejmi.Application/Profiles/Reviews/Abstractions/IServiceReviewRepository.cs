using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Domain.Profiles.Players;

namespace Zagejmi.Application.Profiles.Reviews.Abstractions;

/// <summary>
///     Persists reviews of completed orders (one per order).
/// </summary>
public interface IServiceReviewRepository
{
    /// <summary>
    ///     Gets the review of an order.
    /// </summary>
    /// <param name="orderId">The order.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The review, or <see langword="null" />.</returns>
    Task<ServiceReview?> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Stores the review, replacing an earlier review of the same order (rating, text and
    ///     <see cref="ServiceReview.UpdatedAt" />; the original ID and creation time stay).
    /// </summary>
    /// <param name="review">The review.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes when the review is stored.</returns>
    Task UpsertAsync(ServiceReview review, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes the review of an order.
    /// </summary>
    /// <param name="orderId">The order.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true" /> when a review was removed.</returns>
    Task<bool> DeleteByOrderAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a page of the reviews of a player's services, newest first, with the authors' names and pictures.
    /// </summary>
    /// <param name="playerProfileId">The player.</param>
    /// <param name="skip">How many to skip.</param>
    /// <param name="take">How many to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The page and the total count.</returns>
    Task<PagedResult<ServiceReviewItem>> GetForPlayerAsync(
        Guid playerProfileId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the average rating and review count of each given service. Services without reviews are left out.
    /// </summary>
    /// <param name="serviceIds">The services.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The summaries keyed by service ID.</returns>
    Task<IReadOnlyDictionary<Guid, RatingSummary>> GetSummariesAsync(
        IReadOnlyCollection<Guid> serviceIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets which of the given orders already have a review.
    /// </summary>
    /// <param name="orderIds">The orders.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The reviewed order IDs.</returns>
    Task<IReadOnlySet<Guid>> GetReviewedOrderIdsAsync(
        IReadOnlyCollection<Guid> orderIds,
        CancellationToken cancellationToken = default);
}
