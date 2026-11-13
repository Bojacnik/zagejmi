using System;
using System.IO;

using Zagejmi.Application.Images.Abstractions;

using Zagejmi.Domain.Profiles;

namespace Zagejmi.Application.Profiles.SetProfileImage;

/// <summary>
///     Represents a command to change the profile picture of the user's customer or player profile.
/// </summary>
public sealed class SetProfileImageCommand
{
    /// <summary>
    ///     Gets the user ID.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    ///     Gets which of the user's profiles gets the picture.
    /// </summary>
    public required ProfileType Profile { get; init; }

    /// <summary>
    ///     Gets the uploaded image (JPG, PNG or WebP).
    /// </summary>
    public required Stream Content { get; init; }

    /// <summary>
    ///     Gets the part of the image the user chose in the cropper, or <see langword="null" /> for the center.
    /// </summary>
    public CropRegion? Crop { get; init; }
}
