namespace Zagejmi.Application.Profiles.Reviews.GetPlayerReviews;

/// <summary>
///     Whether the viewer can write a review of the player.
/// </summary>
public enum ReviewEligibility
{
    /// <summary>
    ///     The viewer can write (or change) a review.
    /// </summary>
    CanReview,

    /// <summary>
    ///     The viewer is not signed in.
    /// </summary>
    SignedOut,

    /// <summary>
    ///     The viewer is in ghost mode (browse only).
    /// </summary>
    GhostMode,

    /// <summary>
    ///     The player profile is the viewer's own.
    /// </summary>
    OwnProfile,

    /// <summary>
    ///     The viewer has no completed order with the player yet.
    /// </summary>
    NoCompletedOrder,
}
