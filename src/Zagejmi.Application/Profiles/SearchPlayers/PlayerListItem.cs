using System;

using Zagejmi.Application.Profiles.Reviews;

namespace Zagejmi.Application.Profiles.SearchPlayers;

/// <summary>
///     A player as shown in the player directory.
/// </summary>
/// <param name="ProfileId">The player profile ID (used in /players/{id}).</param>
/// <param name="DisplayName">The public display name.</param>
/// <param name="Description">The short public description, if any.</param>
/// <param name="CreatedAt">When the player profile was created.</param>
/// <param name="ProfileImageId">The profile picture (logo), if any.</param>
/// <param name="Rating">The average rating, or <see langword="null" /> when nobody has reviewed the player yet.</param>
public sealed record PlayerListItem(
    Guid ProfileId,
    string DisplayName,
    string? Description,
    DateTimeOffset CreatedAt,
    Guid? ProfileImageId,
    RatingSummary? Rating = null);
