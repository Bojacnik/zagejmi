using System;

namespace Zagejmi.Infrastructure.Profiles.Records;

/// <summary>
///     Database row of a customer's review of a player (profiles.PlayerReviews).
/// </summary>
public record PlayerReviewRecord
{
    /// <summary>
    ///     Gets the review ID.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     Gets the reviewed player profile.
    /// </summary>
    public Guid PlayerProfileId { get; init; }

    /// <summary>
    ///     Gets the customer profile that wrote the review.
    /// </summary>
    public Guid AuthorProfileId { get; init; }

    /// <summary>
    ///     Gets or sets the rating, 1–5 stars.
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    ///     Gets or sets the optional text.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    ///     Gets when the review was first written.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    ///     Gets or sets when the review was last changed.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
