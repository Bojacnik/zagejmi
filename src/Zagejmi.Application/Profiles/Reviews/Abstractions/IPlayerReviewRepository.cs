using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Domain.Profiles.Players;

namespace Zagejmi.Application.Profiles.Reviews.Abstractions;

/// <summary>
///     Persists customers' reviews of players.
/// </summary>
public interface IPlayerReviewRepository
{
    /// <summary>
    ///     Gets the review the customer profile wrote about the player.
    /// </summary>
    /// <param name="playerProfileId">The reviewed player profile.</param>
    /// <param name="authorProfileId">The customer profile that wrote it.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The review, or <see langword="null" /> when there is none.</returns>
    Task<PlayerReview?> GetAsync(Guid playerProfileId, Guid authorProfileId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Stores the review, replacing the author's previous review of the same player (rating, text and
    ///     <see cref="PlayerReview.UpdatedAt" />; the original ID and creation time stay).
    /// </summary>
    /// <param name="review">The review.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes when the review is stored.</returns>
    Task UpsertAsync(PlayerReview review, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes the review the customer profile wrote about the player.
    /// </summary>
    /// <param name="playerProfileId">The reviewed player profile.</param>
    /// <param name="authorProfileId">The customer profile that wrote it.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true" /> when a review was removed.</returns>
    Task<bool> DeleteAsync(Guid playerProfileId, Guid authorProfileId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a page of the player's reviews, newest first, with the authors' names and pictures.
    /// </summary>
    /// <param name="playerProfileId">The reviewed player profile.</param>
    /// <param name="skip">How many reviews to skip.</param>
    /// <param name="take">How many reviews to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The page and the total number of the player's reviews.</returns>
    Task<PagedResult<PlayerReviewItem>> GetForPlayerAsync(
        Guid playerProfileId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the average rating and review count of each given player. Players without reviews are left out.
    /// </summary>
    /// <param name="playerProfileIds">The player profiles.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The summaries keyed by player profile ID.</returns>
    Task<IReadOnlyDictionary<Guid, RatingSummary>> GetSummariesAsync(
        IReadOnlyCollection<Guid> playerProfileIds,
        CancellationToken cancellationToken = default);
}
