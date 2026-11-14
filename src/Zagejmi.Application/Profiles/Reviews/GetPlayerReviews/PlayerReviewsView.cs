using System.Collections.Generic;

namespace Zagejmi.Application.Profiles.Reviews.GetPlayerReviews;

/// <summary>
///     A player's reviews as the viewer sees them.
/// </summary>
/// <param name="Summary">The average rating over all reviews.</param>
/// <param name="Items">The page of reviews, newest first.</param>
/// <param name="TotalCount">How many reviews the player has.</param>
/// <param name="Mine">The viewer's own review of the player, if any.</param>
/// <param name="Eligibility">Whether the viewer can write a review.</param>
public sealed record PlayerReviewsView(
    RatingSummary Summary,
    IReadOnlyList<PlayerReviewItem> Items,
    int TotalCount,
    PlayerReviewItem? Mine,
    ReviewEligibility Eligibility);
