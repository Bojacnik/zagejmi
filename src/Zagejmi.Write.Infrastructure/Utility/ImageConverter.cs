using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace Zagejmi.Write.Infrastructure.Utility;

public class ImageConverter : ValueConverter<Image, byte[]>
{
    public ImageConverter()
        : base(
            image => ImageToByteArray(image),
            bytes => ByteArrayToImage(bytes))
    {
    }

    private static byte[] ImageToByteArray(Image image)
    {
        using MemoryStream ms = new MemoryStream();
        image.Save(ms, new PngEncoder());
        return ms.ToArray();
    }

    private static Image ByteArrayToImage(byte[] bytes)
    {
        return Image.Load(bytes);
    }
}
