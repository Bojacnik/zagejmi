using System;

namespace Zagejmi.Application.Profiles.Reviews.GetPlayerReviews;

/// <summary>
///     Reads a player's reviews.
/// </summary>
/// <param name="PlayerProfileId">The player.</param>
/// <param name="ViewerUserId">The signed-in user, or <see langword="null" /> for an anonymous visitor.</param>
/// <param name="Page">1-based page.</param>
/// <param name="PageSize">Reviews per page.</param>
public sealed record GetPlayerReviewsQuery(Guid PlayerProfileId, Guid? ViewerUserId, int Page = 1, int PageSize = 10)
{
    /// <summary>
    ///     The largest page.
    /// </summary>
    public const int MaxPageSize = 50;
}
