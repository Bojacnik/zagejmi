using System;

namespace Zagejmi.Infrastructure.Profiles.Records;

/// <summary>
///     Represents a player profile record containing information about a player.
/// </summary>
public record PlayerProfileRecord
{
    /// <summary>
    ///     Gets the unique identifier for the player profile record.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     Gets the identifier of the owning user (users live in a separate DbContext).
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    ///     Gets the time the profile was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    ///     Gets or sets the display name for the player profile.
    /// </summary>
    public required string DisplayName { get; set; }


    /// <summary>
    ///     Gets or sets the identifier of the card profile picture image blob.
    /// </summary>
    public Guid? CardProfilePictureId { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the banner image blob.
    /// </summary>
    public Guid? BannerPictureId { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the services background image blob.
    /// </summary>
    public Guid? BackgroundServicesPictureId { get; set; }

    /// <summary>
    ///     Gets or sets the description for the player profile.
    /// </summary>
    public string? Description { get; set; }
}