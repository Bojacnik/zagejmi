using System;

namespace Zagejmi.Read.Api.Models;

/// <summary>
///     Card-style view model for displaying a user profile.
/// </summary>
public sealed class UserProfileCard
{
    /// <summary>
    ///     Gets the profile identifier.
    /// </summary>
    public required Guid ProfileId { get; init; }

    /// <summary>
    ///     Gets the profile type.
    /// </summary>
    public required ProfileType Type { get; init; }

    /// <summary>
    ///     Gets or sets the display name.
    /// </summary>
    public required string DisplayName { get; set; }

    /// <summary>
    ///     Gets or sets the profile image URL.
    /// </summary>
    public string? ProfileImageUrl { get; set; }

    /// <summary>
    ///     Gets aggregated profile statistics.
    /// </summary>
    public ProfileStats Stats { get; } = new();
}

/// <summary>
///     Aggregate profile statistics shown in profile cards.
/// </summary>
public sealed class ProfileStats
{
    /// <summary>
    ///     Gets or sets the profile level.
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    ///     Gets or sets the reputation score.
    /// </summary>
    public int Reputation { get; set; }

    /// <summary>
    ///     Gets or sets number of completed sessions.
    /// </summary>
    public int CompletedSessions { get; set; }
}