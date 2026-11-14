using System;

namespace Zagejmi.Application.Profiles.Reviews;

/// <summary>
///     A player's average rating.
/// </summary>
/// <param name="Average">The average number of stars (1–5).</param>
/// <param name="Count">How many reviews the average is based on.</param>
public sealed record RatingSummary(double Average, int Count)
{
    /// <summary>
    ///     The summary of a player nobody has reviewed yet.
    /// </summary>
    public static readonly RatingSummary None = new(0, 0);
}

/// <summary>
///     A review as shown on the player's profile.
/// </summary>
/// <param name="AuthorProfileId">The customer profile that wrote it.</param>
/// <param name="AuthorName">The author's display name.</param>
/// <param name="AuthorImageId">The author's profile picture, if any.</param>
/// <param name="Rating">1–5 stars.</param>
/// <param name="Comment">The optional text.</param>
/// <param name="CreatedAt">When it was first written.</param>
/// <param name="UpdatedAt">When it was last changed.</param>
public sealed record PlayerReviewItem(
    Guid AuthorProfileId,
    string AuthorName,
    Guid? AuthorImageId,
    int Rating,
    string? Comment,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    /// <summary>
    ///     Gets a value indicating whether the review was changed after it was written.
    /// </summary>
    public bool IsEdited => this.UpdatedAt > this.CreatedAt;
}
