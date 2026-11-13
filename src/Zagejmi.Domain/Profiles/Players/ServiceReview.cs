using System;

namespace Zagejmi.Domain.Profiles.Players;

/// <summary>
///     Represents a review of a service provided by a player, including the comment, profile ID of the reviewer, date
///     of the review, and the rating given.
/// </summary>
public record ServiceReview
{
    /// <summary>
    ///     Gets or sets the comment provided by the reviewer about the service.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    ///     Gets or sets the unique identifier of the profile that provided the review. This is a nullable GUID, indicating
    ///     that the review may not be associated with a specific profile.
    /// </summary>
    public Guid? ProfileId { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the review was created. This is represented as a DateTimeOffset to include
    ///     timezone information.
    /// </summary>
    public DateTimeOffset Date { get; set; }

    /// <summary>
    ///     Gets or sets the rating given by the reviewer for the service. This is represented as a decimal value, allowing for fractional ratings.
    /// </summary>
    public decimal Rating { get; set; }
}
