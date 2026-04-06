using System.IO;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace Zagejmi.Write.Infrastructure.Utility;

/// <summary>
///     A value converter for converting between Image objects and byte arrays for storage in a database.
/// </summary>
public class ImageConverter : ValueConverter<Image, byte[]>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ImageConverter" /> class, defining the conversion logic for both
    ///     directions: from Image to byte array and from byte array to Image.
    /// </summary>
    public ImageConverter()
        : base(
            image => ImageToByteArray(image),
            bytes => ByteArrayToImage(bytes))
    {
    }

    /// <summary>
    ///     Converts an Image object to a byte array by saving the image in PNG format to a memory stream and then
    ///     returning the byte array representation of the image data. This allows for efficient storage of image data in a
    ///     database while preserving the image quality and format.
    /// </summary>
    /// <param name="image">The Image object to be converted to a byte array.</param>
    /// <returns>A byte array representing the image data in PNG format.</returns>
    private static byte[] ImageToByteArray(Image image)
    {
        using MemoryStream ms = new();
        ms.ReadTimeout = 0;
        ms.WriteTimeout = 0;
        ms.Capacity = 0;
        ms.Position = 0;
        image.Save(ms, new PngEncoder());
        return ms.ToArray();
    }

    /// <summary>
    ///     Converts a byte array back to an Image object by loading the image data from the byte array. This allows for
    ///     retrieving and reconstructing the original image from its byte array representation when reading from the database.
    ///     The method uses the Image.Load method from the ImageSharp library, which can automatically detect the image format
    ///     based on the byte data, ensuring that the image is correctly reconstructed regardless of its original format.
    /// </summary>
    /// <param name="bytes">The byte array representing the image data, typically retrieved from the database.</param>
    /// <returns>An Image object reconstructed from the byte array data.</returns>
    private static Image ByteArrayToImage(byte[] bytes)
    {
        return Image.Load(bytes);
    }
}