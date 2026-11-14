using System;

namespace Zagejmi.Domain.Profiles.Players;

/// <summary>
///     A customer's review of one completed order of a service: 1–5 whole stars and an optional text. Every completed
///     order (where the service was delivered) can be reviewed once, so a customer who orders often reviews often. The
///     service name is copied from the order, so the review stays readable after the service is renamed or removed.
/// </summary>
public sealed record ServiceReview
{
    /// <summary>
    ///     Gets the review ID.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    ///     Gets the reviewed order (one review per order).
    /// </summary>
    public required Guid OrderId { get; init; }

    /// <summary>
    ///     Gets the ordered service.
    /// </summary>
    public required Guid ServiceId { get; init; }

    /// <summary>
    ///     Gets the service name at order time.
    /// </summary>
    public required string ServiceName { get; init; }

    /// <summary>
    ///     Gets the player who provided the service.
    /// </summary>
    public required Guid PlayerProfileId { get; init; }

    /// <summary>
    ///     Gets the customer profile that wrote the review.
    /// </summary>
    public required Guid AuthorProfileId { get; init; }

    /// <summary>
    ///     Gets the rating, <see cref="PlayerReview.MinRating" /> to <see cref="PlayerReview.MaxRating" /> stars.
    /// </summary>
    public required int Rating { get; init; }

    /// <summary>
    ///     Gets the optional text (at most <see cref="PlayerReview.CommentMaxLength" /> characters).
    /// </summary>
    public string? Comment { get; init; }

    /// <summary>
    ///     Gets when the review was first written.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    ///     Gets when the review was last changed.
    /// </summary>
    public required DateTimeOffset UpdatedAt { get; init; }
}
