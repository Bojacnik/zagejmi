namespace Zagejmi.Application.Profiles.Reviews;

/// <summary>
///     Settings of reviews (configuration section <c>Reviews</c>).
/// </summary>
public sealed class ReviewOptions
{
    /// <summary>
    ///     The configuration section name.
    /// </summary>
    public const string SectionName = "Reviews";

    /// <summary>
    ///     Gets or sets how many reviews a service (or recommendations a player) needs before its average is shown on
    ///     cards in listings. Fewer would let one review define the card.
    /// </summary>
    public int MinReviewsToShowRating { get; set; } = 5;

    /// <summary>
    ///     Gets the summary to show on a card, or <see langword="null" /> while there are too few reviews.
    /// </summary>
    /// <param name="summary">The full summary, if any.</param>
    /// <returns>The summary, or <see langword="null" />.</returns>
    public RatingSummary? Visible(RatingSummary? summary) =>
        summary is { } s && s.Count >= this.MinReviewsToShowRating ? s : null;
}
