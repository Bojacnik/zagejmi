using System;

using Zagejmi.Domain.Games;

namespace Zagejmi.Application.Profiles.SearchServices;

/// <summary>
///     A service on the Služby page, with the player who offers it.
/// </summary>
/// <param name="ServiceId">The service ID.</param>
/// <param name="PlayerProfileId">The player profile that offers the service.</param>
/// <param name="PlayerDisplayName">The player's display name.</param>
/// <param name="PlayerImageId">The player's profile picture, if any.</param>
/// <param name="Name">The service name.</param>
/// <param name="Description">What the service includes.</param>
/// <param name="Price">The price in Goins.</param>
/// <param name="PriceUnit">What the price is for, e.g. "/ hodina".</param>
/// <param name="Category">Optional category.</param>
/// <param name="GameKey">The game key, or <see langword="null" />.</param>
/// <param name="RankKey">The player's rank key in the game, or <see langword="null" />.</param>
/// <param name="PlayStyles">Competitive, casual or both.</param>
/// <param name="ImageId">The service image, if any.</param>
/// <param name="CreatedAt">When the service was created.</param>
public sealed record ServiceListItem(
    Guid ServiceId,
    Guid PlayerProfileId,
    string PlayerDisplayName,
    Guid? PlayerImageId,
    string Name,
    string Description,
    long Price,
    string? PriceUnit,
    string? Category,
    string? GameKey,
    string? RankKey,
    PlayStyles PlayStyles,
    Guid? ImageId,
    DateTimeOffset CreatedAt);
