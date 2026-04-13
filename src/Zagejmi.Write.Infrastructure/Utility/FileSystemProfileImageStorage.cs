using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

using Zagejmi.Write.Application.Abstractions;

namespace Zagejmi.Write.Infrastructure.Utility;

public sealed class FileSystemProfileImageStorage : IProfileImageStorage
{
    private readonly string storageRoot;

    public FileSystemProfileImageStorage()
    {
        this.storageRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Zagejmi",
            "ProfileImages");
        Directory.CreateDirectory(this.storageRoot);
    }

    public async Task<string> SaveProfileImageAsync(
        byte[] imageBytes,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        using Image image = Image.Load(imageBytes);

        image.Mutate(ctx =>
        {
            const int maxEdge = 1024;
            if (image.Width > maxEdge || image.Height > maxEdge)
            {
                ctx.Resize(
                    new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(maxEdge, maxEdge)
                    });
            }
        });

        string blobKey = $"{Guid.CreateVersion7():N}.jpg";
        string filePath = Path.Combine(this.storageRoot, blobKey);

        await using FileStream fileStream = new(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await image.SaveAsJpegAsync(fileStream, new JpegEncoder { Quality = 85 }, cancellationToken);

        return blobKey;
    }

    public async Task<(byte[] Content, string ContentType)?> ReadProfileImageAsync(
        string blobKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(blobKey) || blobKey.Contains("..", StringComparison.Ordinal))
        {
            return null;
        }

        string filePath = Path.Combine(this.storageRoot, blobKey);
        if (!File.Exists(filePath))
        {
            return null;
        }

        byte[] bytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
        return (bytes, "image/jpeg");
    }
}