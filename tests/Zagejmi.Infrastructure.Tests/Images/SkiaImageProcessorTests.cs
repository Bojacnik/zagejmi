using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using SkiaSharp;

using Xunit;

using Zagejmi.Application.Images.Abstractions;
using Zagejmi.Infrastructure.Images.Processing;
using Zagejmi.Shared;

namespace Zagejmi.Infrastructure.Tests.Images;

public sealed class SkiaImageProcessorTests
{
    // Test pictures are split into four solid quadrants so position and rotation can be read back from the colors.
    private static readonly SKColor R = SKColors.Red;
    private static readonly SKColor G = SKColors.Lime;
    private static readonly SKColor B = SKColors.Blue;
    private static readonly SKColor W = SKColors.White;

    private static readonly ImageLimits NoLimits = new(10_000, 10_000);

    private readonly SkiaImageProcessor processor = new();

    public static TheoryData<int, bool, string> Orientations => new()
    {
        // EXIF value, sides swapped, expected quadrants TL TR BL BR of the upright result (stored picture is R G / B W).
        { 1, false, "RGBW" },
        { 2, false, "GRWB" }, // mirrored horizontally
        { 3, false, "WBGR" }, // rotated 180°
        { 4, false, "BWRG" }, // mirrored vertically
        { 5, true, "RBGW" },  // transposed
        { 6, true, "BRWG" },  // rotated 90° clockwise
        { 7, true, "WGBR" },  // transversed
        { 8, true, "GWRB" },  // rotated 90° counter-clockwise
    };

    [Theory]
    [MemberData(nameof(Orientations))]
    public async Task Applies_exif_orientation(int orientation, bool swapsSides, string expected)
    {
        byte[] jpeg = WithExifOrientation(Encode(Quadrants(80, 40), SKEncodedImageFormat.Jpeg), orientation);

        SKBitmap result = await ProcessOk(jpeg, NoLimits);

        Assert.Equal(swapsSides ? (40, 80) : (80, 40), (result.Width, result.Height));
        AssertQuadrants(result, expected);
    }

    [Fact]
    public async Task Output_is_webp_without_metadata()
    {
        byte[] jpeg = WithExifOrientation(Encode(Quadrants(80, 40), SKEncodedImageFormat.Jpeg), 1);

        Result<ProcessedImage> result = await Process(jpeg, NoLimits);

        Assert.True(result.IsSuccess);
        byte[] output = result.Value.Content;
        Assert.Equal("RIFF", Encoding.ASCII.GetString(output, 0, 4));
        Assert.Equal("WEBP", Encoding.ASCII.GetString(output, 8, 4));
        Assert.Equal("image/webp", result.Value.ContentType);
        Assert.DoesNotContain("Exif", Encoding.ASCII.GetString(output), StringComparison.Ordinal);
        Assert.DoesNotContain("EXIF", Encoding.ASCII.GetString(output), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Keeps_png_transparency()
    {
        using SKBitmap source = new(new SKImageInfo(20, 20, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        source.Erase(SKColors.Transparent);

        SKBitmap result = await ProcessOk(Encode(source, SKEncodedImageFormat.Png), NoLimits);

        Assert.Equal(0, result.GetPixel(10, 10).Alpha);
    }

    [Fact]
    public async Task Crops_the_center_when_an_aspect_ratio_is_required_without_a_crop()
    {
        // 400×200 → center 200×200 square (x 100–300): left half red/blue, right half green/white.
        SKBitmap result = await ProcessOk(Encode(Quadrants(400, 200), SKEncodedImageFormat.Png), new ImageLimits(100, 100, 1.0));

        Assert.Equal((100, 100), (result.Width, result.Height));
        AssertQuadrants(result, "RGBW");
    }

    [Fact]
    public async Task Crops_the_region_chosen_by_the_user()
    {
        // The bottom-right quadrant only.
        SKBitmap result = await ProcessOk(
            Encode(Quadrants(400, 200), SKEncodedImageFormat.Png),
            new ImageLimits(1000, 1000, 2.0),
            new CropRegion(0.5, 0.5, 0.5, 0.5));

        Assert.Equal((200, 100), (result.Width, result.Height));
        AssertQuadrants(result, "WWWW");
    }

    [Fact]
    public async Task Crop_refers_to_the_upright_picture()
    {
        // Stored R G / B W rotated clockwise is B R / W G; its top-left quarter is blue.
        byte[] jpeg = WithExifOrientation(Encode(Quadrants(80, 40), SKEncodedImageFormat.Jpeg), 6);

        SKBitmap result = await ProcessOk(jpeg, new ImageLimits(1000, 1000, 0.5), new CropRegion(0, 0, 0.5, 0.5));

        Assert.Equal((20, 40), (result.Width, result.Height));
        AssertQuadrants(result, "BBBB");
    }

    [Fact]
    public async Task Shrinks_large_images_to_the_limits_keeping_the_shape()
    {
        SKBitmap result = await ProcessOk(Encode(Quadrants(3000, 1500), SKEncodedImageFormat.Png), new ImageLimits(300, 300));

        Assert.Equal((300, 150), (result.Width, result.Height));
        AssertQuadrants(result, "RGBW");
    }

    [Fact]
    public async Task Never_enlarges_small_images()
    {
        SKBitmap result = await ProcessOk(Encode(Quadrants(40, 20), SKEncodedImageFormat.Png), new ImageLimits(1600, 1600));

        Assert.Equal((40, 20), (result.Width, result.Height));
    }

    [Fact]
    public async Task Accepts_webp()
    {
        SKBitmap result = await ProcessOk(Encode(Quadrants(40, 20), SKEncodedImageFormat.Webp), NoLimits);

        AssertQuadrants(result, "RGBW");
    }

    [Fact]
    public async Task Rejects_files_that_are_not_images()
    {
        Result<ProcessedImage> result = await Process(Encoding.UTF8.GetBytes("<html>definitely not a picture</html>"), NoLimits);

        Assert.Equal("Image.Invalid", result.Error?.Code);
    }

    [Fact]
    public async Task Rejects_image_formats_that_are_not_allowed()
    {
        byte[] gif = Convert.FromHexString("47494638396101000100800000000000FFFFFF21F90401000000002C00000000010001000002024401003B");

        Result<ProcessedImage> result = await Process(gif, NoLimits);

        Assert.Equal("Image.Invalid", result.Error?.Code);
    }

    [Fact]
    public async Task Rejects_truncated_images()
    {
        byte[] png = Encode(Quadrants(400, 200), SKEncodedImageFormat.Png);

        Result<ProcessedImage> result = await Process(png.AsSpan(0, png.Length / 2).ToArray(), NoLimits);

        Assert.Equal("Image.Invalid", result.Error?.Code);
    }

    [Fact]
    public async Task Rejects_decompression_bombs_before_decoding()
    {
        // Only a header claiming 10 000 × 10 000 pixels; nothing would be left to decode.
        Result<ProcessedImage> result = await Process(PngHeader(10_000, 10_000), NoLimits);

        Assert.Equal("Image.TooManyPixels", result.Error?.Code);
    }

    [Theory]
    [InlineData(double.NaN, 0, 1, 1)]
    [InlineData(-5, -5, 50, 50)]
    [InlineData(0, 0, 1, 1)]
    public void Keeps_the_whole_image_for_missing_or_out_of_range_crops_without_aspect_ratio(double x, double y, double w, double h)
    {
        Assert.Null(SkiaImageProcessor.ToCropRectangle(100, 50, new CropRegion(x, y, w, h), null));
    }

    [Fact]
    public void Forces_the_exact_aspect_ratio_on_a_slightly_off_crop()
    {
        SKRectI? rectangle = SkiaImageProcessor.ToCropRectangle(1000, 1000, new CropRegion(0.1, 0.1, 0.5, 0.26), 2.0);

        Assert.Equal(SKRectI.Create(100, 105, 500, 250), rectangle);
    }

    private static SKBitmap Quadrants(int width, int height)
    {
        SKBitmap bitmap = new(width, height);
        using SKCanvas canvas = new(bitmap);
        using SKPaint paint = new();
        foreach ((SKColor color, int x, int y) in new[] { (R, 0, 0), (G, 1, 0), (B, 0, 1), (W, 1, 1) })
        {
            paint.Color = color;
            canvas.DrawRect(x * width / 2, y * height / 2, width / 2, height / 2, paint);
        }

        return bitmap;
    }

    private static byte[] Encode(SKBitmap bitmap, SKEncodedImageFormat format)
    {
        using (bitmap)
        {
            using SKData data = bitmap.Encode(format, 100);
            return data.ToArray();
        }
    }

    /// <summary>
    ///     Inserts a minimal EXIF block (APP1) with just the orientation tag right after the JPEG start marker.
    /// </summary>
    private static byte[] WithExifOrientation(byte[] jpeg, int orientation)
    {
        byte[] app1 =
        [
            0xFF, 0xE1, 0x00, 0x22,                         // APP1, length 34
            (byte)'E', (byte)'x', (byte)'i', (byte)'f', 0, 0,
            (byte)'M', (byte)'M', 0x00, 0x2A, 0, 0, 0, 8,   // big-endian TIFF header, first IFD at 8
            0x00, 0x01,                                     // one entry
            0x01, 0x12, 0x00, 0x03, 0, 0, 0, 1,             // Orientation, SHORT, count 1
            0x00, (byte)orientation, 0, 0,
            0, 0, 0, 0,                                     // no next IFD
        ];

        byte[] result = new byte[jpeg.Length + app1.Length];
        jpeg.AsSpan(0, 2).CopyTo(result);
        app1.CopyTo(result, 2);
        jpeg.AsSpan(2).CopyTo(result.AsSpan(2 + app1.Length));
        return result;
    }

    private static byte[] PngHeader(int width, int height)
    {
        using MemoryStream png = new();
        png.Write([0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A]);

        byte[] ihdr = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(ihdr, width);
        BinaryPrimitives.WriteInt32BigEndian(ihdr.AsSpan(4), height);
        ihdr[8] = 8; // bit depth
        ihdr[9] = 2; // RGB
        WriteChunk(png, "IHDR", ihdr);
        WriteChunk(png, "IDAT", [0x78, 0x9C, 0x03, 0x00, 0x00, 0x00, 0x00, 0x01]);
        WriteChunk(png, "IEND", []);
        return png.ToArray();
    }

    private static void WriteChunk(Stream stream, string type, byte[] data)
    {
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(length, data.Length);
        stream.Write(length);

        byte[] typeAndData = [.. Encoding.ASCII.GetBytes(type), .. data];
        stream.Write(typeAndData);

        Span<byte> crc = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crc, Crc32(typeAndData));
        stream.Write(crc);
    }

    private static uint Crc32(byte[] bytes)
    {
        uint crc = 0xFFFFFFFF;
        foreach (byte b in bytes)
        {
            crc ^= b;
            for (int i = 0; i < 8; i++)
            {
                crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320 : crc >> 1;
            }
        }

        return ~crc;
    }

    private static void AssertQuadrants(SKBitmap bitmap, string expected)
    {
        (int X, int Y)[] centers =
        [
            (bitmap.Width / 4, bitmap.Height / 4),
            (bitmap.Width * 3 / 4, bitmap.Height / 4),
            (bitmap.Width / 4, bitmap.Height * 3 / 4),
            (bitmap.Width * 3 / 4, bitmap.Height * 3 / 4),
        ];

        for (int i = 0; i < 4; i++)
        {
            SKColor want = expected[i] switch { 'R' => R, 'G' => G, 'B' => B, _ => W };
            SKColor got = bitmap.GetPixel(centers[i].X, centers[i].Y);
            bool close = Math.Abs(want.Red - got.Red) < 40 && Math.Abs(want.Green - got.Green) < 40
                                                           && Math.Abs(want.Blue - got.Blue) < 40;
            Assert.True(close, $"Quadrant {i} ({centers[i].X}, {centers[i].Y}): expected {want}, got {got} (pattern {expected}).");
        }
    }

    private Task<Result<ProcessedImage>> Process(byte[] bytes, ImageLimits limits, CropRegion? crop = null)
    {
        return this.processor.ProcessAsync(new MemoryStream(bytes), limits, crop, TestContext.Current.CancellationToken);
    }

    private async Task<SKBitmap> ProcessOk(byte[] bytes, ImageLimits limits, CropRegion? crop = null)
    {
        Result<ProcessedImage> result = await Process(bytes, limits, crop);
        Assert.True(result.IsSuccess, result.Error?.Message);

        SKBitmap bitmap = SKBitmap.Decode(result.Value.Content);
        Assert.Equal((result.Value.Width, result.Value.Height), (bitmap.Width, bitmap.Height));
        return bitmap;
    }
}
