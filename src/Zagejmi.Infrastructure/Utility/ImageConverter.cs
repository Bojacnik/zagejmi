﻿using System.Drawing;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Zagejmi.Infrastructure.Utility;

public class ImageConverter : ValueConverter<Image, byte[]>
{
    // TODO: Make platform independent
    public ImageConverter()
        : base(
            image => ImageToByteArray(image),
            bytes => ByteArrayToImage(bytes))
    {
    }

    private static byte[] ImageToByteArray(Image image)
    {
        using var ms = new MemoryStream();
        // platform dependent
        // TODO: Make platform independent
        image.Save(ms, image.RawFormat);
        return ms.ToArray();
    }

    private static Image ByteArrayToImage(byte[] bytes)
    {
        using var ms = new MemoryStream(bytes);
        // platform dependent
        // TODO: Make platform independent
        return Image.FromStream(ms);
    }
}