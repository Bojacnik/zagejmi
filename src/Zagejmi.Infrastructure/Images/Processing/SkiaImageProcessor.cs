using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using SkiaSharp;

using Zagejmi.Application.Images.Abstractions;
using Zagejmi.Shared;

namespace Zagejmi.Infrastructure.Images.Processing;

/// <summary>
///     SkiaSharp implementation of <see cref="IImageProcessor" />. Accepts JPEG, PNG and WebP (checked from the file
///     content, not the name), converts colors to sRGB and stores everything as WebP. The output is encoded from pixels
///     only, so no metadata (e.g. the GPS position) ever leaves this class.
/// </summary>
internal sealed class SkiaImageProcessor : IImageProcessor
{
    // Refuse "decompression bombs": tiny files that decode to gigantic bitmaps.
    private const long MaxSourcePixels = 50_000_000;

    private const int WebpQuality = 85;

    private static readonly ErrorDetail NotAnImage = ErrorDetail.Validation(
        "Image.Invalid",
        "Soubor není platný obrázek. Nahrajte prosím JPG, PNG nebo WebP.");

    private static readonly ErrorDetail TooManyPixels = ErrorDetail.Validation(
        "Image.TooManyPixels",
        "Obrázek má příliš velké rozměry. Zmenšete ho prosím pod 50 megapixelů.");

    /// <inheritdoc />
    public async Task<Result<ProcessedImage>> ProcessAsync(
        Stream input,
        ImageLimits limits,
        CropRegion? crop,
        CancellationToken cancellationToken = default)
    {
        byte[] bytes;
        using (MemoryStream buffer = new())
        {
            await input.CopyToAsync(buffer, cancellationToken);
            bytes = buffer.ToArray();
        }

        return Process(bytes, limits, crop, cancellationToken);
    }

    /// <summary>
    ///     Turns the user's crop (fractions) into a pixel rectangle with exactly the required aspect ratio. Values from
    ///     the browser are clamped, so a tampered or slightly-off crop can never fall outside the image. Without a crop,
    ///     the center of the image is used when an aspect ratio is required.
    /// </summary>
    /// <returns>The rectangle to keep, or <see langword="null" /> to keep the whole image.</returns>
    internal static SKRectI? ToCropRectangle(int imageWidth, int imageHeight, CropRegion? crop, double? aspectRatio)
    {
        double x = 0, y = 0, width = imageWidth, height = imageHeight;

        if (crop is not null && double.IsFinite(crop.X) && double.IsFinite(crop.Y)
            && double.IsFinite(crop.Width) && double.IsFinite(crop.Height))
        {
            double left = Math.Clamp(crop.X, 0, 1) * imageWidth;
            double top = Math.Clamp(crop.Y, 0, 1) * imageHeight;
            double right = Math.Clamp(crop.X + crop.Width, 0, 1) * imageWidth;
            double bottom = Math.Clamp(crop.Y + crop.Height, 0, 1) * imageHeight;

            if (right - left >= 1 && bottom - top >= 1)
            {
                (x, y, width, height) = (left, top, right - left, bottom - top);
            }
        }
        else if (aspectRatio is null)
        {
            return null;
        }

        if (aspectRatio is { } ratio and > 0)
        {
            // Shrink the longer side around its center so the shape is exact.
            if (width / height > ratio)
            {
                double newWidth = height * ratio;
                x += (width - newWidth) / 2;
                width = newWidth;
            }
            else
            {
                double newHeight = width / ratio;
                y += (height - newHeight) / 2;
                height = newHeight;
            }
        }

        int px = (int)Math.Round(x);
        int py = (int)Math.Round(y);
        int pw = Math.Max(1, Math.Min((int)Math.Round(width), imageWidth - px));
        int ph = Math.Max(1, Math.Min((int)Math.Round(height), imageHeight - py));

        return px == 0 && py == 0 && pw == imageWidth && ph == imageHeight
            ? null
            : SKRectI.Create(px, py, pw, ph);
    }

    /// <summary>
    ///     The matrix that turns the stored pixels into the upright picture described by the EXIF orientation.
    /// </summary>
    /// <param name="origin">The EXIF orientation.</param>
    /// <param name="width">The stored width.</param>
    /// <param name="height">The stored height.</param>
    /// <returns>The transform, or <see langword="null" /> when the pixels are already upright.</returns>
    internal static SKMatrix? OrientationMatrix(SKEncodedOrigin origin, int width, int height)
    {
        // x' = ScaleX * x + SkewX * y + TransX;  y' = SkewY * x + ScaleY * y + TransY
        return origin switch
        {
            SKEncodedOrigin.TopRight => new SKMatrix(-1, 0, width, 0, 1, 0, 0, 0, 1),         // mirror horizontally
            SKEncodedOrigin.BottomRight => new SKMatrix(-1, 0, width, 0, -1, height, 0, 0, 1), // rotate 180°
            SKEncodedOrigin.BottomLeft => new SKMatrix(1, 0, 0, 0, -1, height, 0, 0, 1),       // mirror vertically
            SKEncodedOrigin.LeftTop => new SKMatrix(0, 1, 0, 1, 0, 0, 0, 0, 1),                // transpose
            SKEncodedOrigin.RightTop => new SKMatrix(0, -1, height, 1, 0, 0, 0, 0, 1),         // rotate 90° clockwise
            SKEncodedOrigin.RightBottom => new SKMatrix(0, -1, height, -1, 0, width, 0, 0, 1), // transverse
            SKEncodedOrigin.LeftBottom => new SKMatrix(0, 1, 0, -1, 0, width, 0, 0, 1),        // rotate 90° counter-clockwise
            _ => null,
        };
    }

    private static Result<ProcessedImage> Process(
        byte[] bytes,
        ImageLimits limits,
        CropRegion? crop,
        CancellationToken cancellationToken)
    {
        using SKData data = SKData.CreateCopy(bytes);
        using SKCodec? codec = SKCodec.Create(data);

        bool allowed = codec?.EncodedFormat is SKEncodedImageFormat.Jpeg or SKEncodedImageFormat.Png
            or SKEncodedImageFormat.Webp;
        if (codec is null || !allowed)
        {
            return Result<ProcessedImage>.Failure(NotAnImage);
        }

        // Only the header has been read so far; check the size before allocating the bitmap.
        int storedWidth = codec.Info.Width;
        int storedHeight = codec.Info.Height;
        if (storedWidth <= 0 || storedHeight <= 0)
        {
            return Result<ProcessedImage>.Failure(NotAnImage);
        }

        if ((long)storedWidth * storedHeight > MaxSourcePixels)
        {
            return Result<ProcessedImage>.Failure(TooManyPixels);
        }

        cancellationToken.ThrowIfCancellationRequested();

        using SKImage? decoded = Decode(codec);
        if (decoded is null)
        {
            return Result<ProcessedImage>.Failure(NotAnImage);
        }

        cancellationToken.ThrowIfCancellationRequested();

        // Phones store rotation in EXIF; bake it in first. Browsers show photos already rotated, so the crop the user
        // chose refers to the rotated image.
        using SKImage? rotated = Orient(decoded, codec.EncodedOrigin);
        SKImage upright = rotated ?? decoded;

        SKRectI source = ToCropRectangle(upright.Width, upright.Height, crop, limits.AspectRatio)
                         ?? SKRectI.Create(0, 0, upright.Width, upright.Height);

        (int targetWidth, int targetHeight) = FitWithin(source.Width, source.Height, limits.MaxWidth, limits.MaxHeight);

        cancellationToken.ThrowIfCancellationRequested();

        using SKImage result = Resize(upright, source, targetWidth, targetHeight);
        using SKData? encoded = result.Encode(SKEncodedImageFormat.Webp, WebpQuality);
        if (encoded is null)
        {
            throw new InvalidOperationException("Skia could not encode the image as WebP.");
        }

        return new ProcessedImage(encoded.ToArray(), "image/webp", "webp", result.Width, result.Height);
    }

    /// <summary>
    ///     Decodes the stored pixels, converting any embedded color profile (e.g. Display P3 from iPhones) to sRGB.
    /// </summary>
    private static SKImage? Decode(SKCodec codec)
    {
        SKAlphaType alpha = codec.Info.AlphaType == SKAlphaType.Opaque ? SKAlphaType.Opaque : SKAlphaType.Premul;
        SKImageInfo info = new(codec.Info.Width, codec.Info.Height, SKColorType.Rgba8888, alpha, SKColorSpace.CreateSrgb());

        using SKBitmap bitmap = new(info);
        SKCodecResult status = codec.GetPixels(info, bitmap.GetPixels());
        if (status != SKCodecResult.Success)
        {
            return null;
        }

        bitmap.SetImmutable();
        return SKImage.FromBitmap(bitmap);
    }

    /// <returns>The upright copy, or <see langword="null" /> when <paramref name="image" /> is already upright.</returns>
    private static SKImage? Orient(SKImage image, SKEncodedOrigin origin)
    {
        if (OrientationMatrix(origin, image.Width, image.Height) is not { } matrix)
        {
            return null;
        }

        bool swapsSides = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop
            or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
        int width = swapsSides ? image.Height : image.Width;
        int height = swapsSides ? image.Width : image.Height;

        using SKSurface surface = CreateSurface(width, height, image.AlphaType);
        surface.Canvas.SetMatrix(matrix);

        // Quarter turns and mirrors map pixels one to one: no filtering needed.
        surface.Canvas.DrawImage(image, 0, 0, new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None));
        return surface.Snapshot();
    }

    /// <summary>
    ///     Scales <paramref name="source" /> of <paramref name="image" /> to the target size. Big reductions are done in
    ///     halving steps (each a clean 2×2 average) so fine detail does not alias; the last step uses a cubic filter.
    /// </summary>
    private static SKImage Resize(SKImage image, SKRectI source, int targetWidth, int targetHeight)
    {
        SKImage current = image;
        SKRectI currentSource = source;
        bool ownsCurrent = false;

        try
        {
            while (currentSource.Width / 2 >= targetWidth && currentSource.Height / 2 >= targetHeight)
            {
                SKImage half = Draw(
                    current,
                    currentSource,
                    currentSource.Width / 2,
                    currentSource.Height / 2,
                    new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None));

                if (ownsCurrent)
                {
                    current.Dispose();
                }

                current = half;
                ownsCurrent = true;
                currentSource = SKRectI.Create(0, 0, half.Width, half.Height);
            }

            return Draw(current, currentSource, targetWidth, targetHeight, new SKSamplingOptions(SKCubicResampler.Mitchell));
        }
        finally
        {
            if (ownsCurrent)
            {
                current.Dispose();
            }
        }
    }

    private static SKImage Draw(SKImage image, SKRectI source, int width, int height, SKSamplingOptions sampling)
    {
        using SKSurface surface = CreateSurface(width, height, image.AlphaType);
        using SKPaint paint = new() { BlendMode = SKBlendMode.Src };
        surface.Canvas.DrawImage(image, SKRect.Create(source.Left, source.Top, source.Width, source.Height), SKRect.Create(0, 0, width, height), sampling, paint);
        return surface.Snapshot();
    }

    private static SKSurface CreateSurface(int width, int height, SKAlphaType alpha)
    {
        SKImageInfo info = new(width, height, SKColorType.Rgba8888, alpha == SKAlphaType.Opaque ? SKAlphaType.Opaque : SKAlphaType.Premul, SKColorSpace.CreateSrgb());
        SKSurface? surface = SKSurface.Create(info);
        if (surface is null)
        {
            throw new InvalidOperationException($"Skia could not allocate a {width}×{height} surface.");
        }

        surface.Canvas.Clear(SKColors.Transparent);
        return surface;
    }

    /// <summary>
    ///     The largest size with the same shape that fits the limits. Images are never enlarged.
    /// </summary>
    private static (int Width, int Height) FitWithin(int width, int height, int maxWidth, int maxHeight)
    {
        double scale = Math.Min(1, Math.Min((double)maxWidth / width, (double)maxHeight / height));
        return (Math.Max(1, (int)Math.Round(width * scale)), Math.Max(1, (int)Math.Round(height * scale)));
    }
}
