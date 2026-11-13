using System;
using System.IO;

using Zagejmi.Application.Images.Abstractions;

namespace Zagejmi.Application.Profiles.AddPlayerService;

/// <summary>
///     Represents a command to add a service to the user's player profile.
/// </summary>
public sealed class AddPlayerServiceCommand
{
    /// <summary>
    ///     Gets the user ID.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    ///     Gets the service name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     Gets what the service includes.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    ///     Gets an optional category (e.g. a game).
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    ///     Gets the price in Goins.
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    ///     Gets what the price is for, e.g. "/ hodina". Optional.
    /// </summary>
    public string? PriceUnit { get; init; }

    /// <summary>
    ///     Gets an optional image (JPG, PNG or WebP).
    /// </summary>
    public Stream? Image { get; init; }

    /// <summary>
    ///     Gets the part of <see cref="Image" /> the user chose in the cropper, or <see langword="null" /> for the center.
    /// </summary>
    public CropRegion? ImageCrop { get; init; }
}
