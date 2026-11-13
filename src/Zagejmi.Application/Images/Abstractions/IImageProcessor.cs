using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Shared;

namespace Zagejmi.Application.Images.Abstractions;

/// <summary>
///     Validates and normalizes uploaded images before they are stored.
/// </summary>
public interface IImageProcessor
{
    /// <summary>
    ///     Decodes the image, applies the EXIF orientation, crops it, strips all metadata (e.g. GPS position), shrinks
    ///     it to fit <paramref name="limits" /> and re-encodes it.
    /// </summary>
    /// <param name="input">The uploaded file.</param>
    /// <param name="limits">The largest allowed output size and the required aspect ratio.</param>
    /// <param name="crop">
    ///     The part of the (correctly rotated) image to keep, chosen by the user. When <see langword="null" /> and the
    ///     limits require an aspect ratio, the center of the image is used.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The processed image, or a validation failure when the file is not a supported image.</returns>
    Task<Result<ProcessedImage>> ProcessAsync(
        Stream input,
        ImageLimits limits,
        CropRegion? crop,
        CancellationToken cancellationToken = default);
}

/// <summary>
///     An image ready to be stored.
/// </summary>
/// <param name="Content">The encoded bytes.</param>
/// <param name="ContentType">The MIME type of <paramref name="Content" />.</param>
/// <param name="Extension">The file extension without a dot.</param>
/// <param name="Width">The width in pixels.</param>
/// <param name="Height">The height in pixels.</param>
public sealed record ProcessedImage(byte[] Content, string ContentType, string Extension, int Width, int Height);

/// <summary>
///     The part of an image to keep, as fractions (0–1) of the width and height of the correctly rotated image.
///     Fractions keep the region valid no matter how large the original is.
/// </summary>
/// <param name="X">Left edge.</param>
/// <param name="Y">Top edge.</param>
/// <param name="Width">Width.</param>
/// <param name="Height">Height.</param>
public sealed record CropRegion(double X, double Y, double Width, double Height);

/// <summary>
///     The largest size an image is shrunk to (images are never enlarged) and, optionally, its required shape.
/// </summary>
/// <param name="MaxWidth">The maximum width in pixels.</param>
/// <param name="MaxHeight">The maximum height in pixels.</param>
/// <param name="AspectRatio">The required width / height ratio, or <see langword="null" /> to keep the original.</param>
public sealed record ImageLimits(int MaxWidth, int MaxHeight, double? AspectRatio = null)
{
    /// <summary>
    ///     Profile pictures: square, shown as a circle.
    /// </summary>
    public static readonly ImageLimits ProfilePicture = new(1024, 1024, 1.0);

    /// <summary>
    ///     Player banners: 4:1, shown in that ratio everywhere so the user's crop is what others see.
    /// </summary>
    public static readonly ImageLimits Banner = new(2560, 640, 4.0);

    /// <summary>
    ///     Service images: 16:9, shown in that ratio on service cards and in the player detail.
    /// </summary>
    public static readonly ImageLimits Service = new(1600, 900, 16.0 / 9.0);
}
