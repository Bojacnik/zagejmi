using System;
using System.IO;

using Zagejmi.Application.Images.Abstractions;
using Zagejmi.Domain.Games;

namespace Zagejmi.Application.Profiles.UpdatePlayerService;

/// <summary>
///     Represents a command to edit one of the user's player services.
/// </summary>
public sealed class UpdatePlayerServiceCommand
{
    /// <summary>
    ///     Gets the user ID.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    ///     Gets the service to edit.
    /// </summary>
    public required Guid ServiceId { get; init; }

    /// <summary>
    ///     Gets the service name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     Gets what the service includes.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    ///     Gets an optional category (e.g. "Koučink").
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    ///     Gets the game the service is for (a <see cref="Zagejmi.Domain.Games.GameCatalog" /> key), or
    ///     <see langword="null" /> when it is not tied to a game.
    /// </summary>
    public string? GameKey { get; init; }

    /// <summary>
    ///     Gets the player's rank in the game (a rank key), or <see langword="null" />.
    /// </summary>
    public string? RankKey { get; init; }

    /// <summary>
    ///     Gets whether the player offers competitive play, casual play or both. Required when a game is set.
    /// </summary>
    public PlayStyles PlayStyles { get; init; }

    /// <summary>
    ///     Gets the price in whole Goins.
    /// </summary>
    public long Price { get; init; }

    /// <summary>
    ///     Gets what the price is for, e.g. "/ hodina". Optional.
    /// </summary>
    public string? PriceUnit { get; init; }

    /// <summary>
    ///     Gets a new image (JPG, PNG or WebP), or <see langword="null" /> to keep the current one.
    /// </summary>
    public Stream? NewImage { get; init; }

    /// <summary>
    ///     Gets the part of <see cref="NewImage" /> the user chose in the cropper, or <see langword="null" /> for the center.
    /// </summary>
    public CropRegion? ImageCrop { get; init; }

    /// <summary>
    ///     Gets a value indicating whether to remove the current image (ignored when <see cref="NewImage" /> is set).
    /// </summary>
    public bool RemoveImage { get; init; }
}
