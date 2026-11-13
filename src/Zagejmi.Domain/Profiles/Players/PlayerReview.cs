using System;

namespace Zagejmi.Domain.Profiles.Players;

/// <summary>
///     A review left on a player's profile.
/// </summary>
public record PlayerReview
{
    /// <summary>
    ///     Gets or sets the optional review text.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the profile that wrote the review.
    /// </summary>
    public Guid? ProfileId { get; set; }

    /// <summary>
    ///     Gets or sets when the review was written.
    /// </summary>
    public DateTimeOffset Date { get; set; }

    /// <summary>
    ///     Gets or sets the rating given.
    /// </summary>
    public decimal Rating { get; set; }
}
