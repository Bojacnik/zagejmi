using System;
using System.Collections.Generic;

using Zagejmi.Domain.Auth;

namespace Zagejmi.Application.Profiles.GetPlayerDetail;

/// <summary>
///     Public detail of a player, shown in the player popup on the Hráči page.
/// </summary>
/// <param name="ProfileId">The player profile ID.</param>
/// <param name="DisplayName">The public display name.</param>
/// <param name="Description">The public description, if any.</param>
/// <param name="CreatedAt">When the player profile was created.</param>
/// <param name="GrammaticalGender">How to address the player (Hráč vs. Hráčka).</param>
/// <param name="ProfileImageId">The profile picture (logo), if any.</param>
/// <param name="BannerImageId">The banner, if any.</param>
/// <param name="Services">The services the player offers.</param>
public sealed record PlayerDetail(
    Guid ProfileId,
    string DisplayName,
    string? Description,
    DateTimeOffset CreatedAt,
    GrammaticalGender GrammaticalGender,
    Guid? ProfileImageId,
    Guid? BannerImageId,
    IReadOnlyList<PlayerServiceItem> Services);

/// <summary>
///     A service offered by a player.
/// </summary>
/// <param name="ServiceId">The service ID.</param>
/// <param name="Name">The service name.</param>
/// <param name="Description">What the service includes.</param>
/// <param name="Price">The price in Goins.</param>
/// <param name="PriceUnit">What the price is for, e.g. "/ hodina".</param>
/// <param name="Category">Optional category (e.g. a game).</param>
/// <param name="ImageId">The service image, if any.</param>
/// <param name="IsAvailable">Whether the service can be ordered right now.</param>
public sealed record PlayerServiceItem(
    Guid ServiceId,
    string Name,
    string Description,
    decimal Price,
    string? PriceUnit,
    string? Category,
    Guid? ImageId,
    bool IsAvailable);
