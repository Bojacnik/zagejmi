using System;
using System.IO;

using Zagejmi.Application.Images.Abstractions;

namespace Zagejmi.Application.Profiles.SetPlayerBanner;

/// <summary>
///     Represents a command to change the banner of the user's player profile.
/// </summary>
public sealed class SetPlayerBannerCommand
{
    /// <summary>
    ///     Gets the user ID.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    ///     Gets the uploaded image (JPG, PNG or WebP).
    /// </summary>
    public required Stream Content { get; init; }

    /// <summary>
    ///     Gets the part of the image the user chose in the cropper, or <see langword="null" /> for the center.
    /// </summary>
    public CropRegion? Crop { get; init; }
}
