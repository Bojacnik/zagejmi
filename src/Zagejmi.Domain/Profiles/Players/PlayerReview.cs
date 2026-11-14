using System;

namespace Zagejmi.Domain.Profiles.Players;

/// <summary>
///     A customer's review of a player: 1–5 whole stars and an optional text. A customer profile reviews each player at
///     most once and can change or remove the review later.
/// </summary>
public sealed record PlayerReview
{
    /// <summary>
    ///     The lowest rating.
    /// </summary>
    public const int MinRating = 1;

    /// <summary>
    ///     The highest rating.
    /// </summary>
    public const int MaxRating = 5;

    /// <summary>
    ///     The longest allowed review text.
    /// </summary>
    public const int CommentMaxLength = 1000;

    /// <summary>
    ///     Gets the review ID.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    ///     Gets the reviewed player profile.
    /// </summary>
    public required Guid PlayerProfileId { get; init; }

    /// <summary>
    ///     Gets the customer profile that wrote the review.
    /// </summary>
    public required Guid AuthorProfileId { get; init; }

    /// <summary>
    ///     Gets the rating, <see cref="MinRating" /> to <see cref="MaxRating" /> stars.
    /// </summary>
    public required int Rating { get; init; }

    /// <summary>
    ///     Gets the optional review text.
    /// </summary>
    public string? Comment { get; init; }

    /// <summary>
    ///     Gets when the review was first written.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    ///     Gets when the review was last changed (equal to <see cref="CreatedAt" /> until it is edited).
    /// </summary>
    public required DateTimeOffset UpdatedAt { get; init; }
}
