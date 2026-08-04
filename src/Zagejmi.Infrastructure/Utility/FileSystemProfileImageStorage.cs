using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

using Zagejmi.Application.Abstractions;
using Zagejmi.Infrastructure.Ctx;

namespace Zagejmi.Infrastructure.Utility;

/// <summary>
///     Write-side database implementation for persisting and reading images.
/// </summary>
public sealed class FileSystemProfileImageStorage : IProfileImageStorage
{
    private readonly ZagejmiContext context;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileSystemProfileImageStorage" /> class.
    /// </summary>
    public FileSystemProfileImageStorage(ZagejmiContext context)
    {
        this.context = context;
    }

    /// <summary>
    ///     Saves an uploaded image as a normalized JPEG and returns its generated blob key.
    /// </summary>
    /// <param name="imageBytes">Raw uploaded image bytes.</param>
    /// <param name="contentType">Original uploaded content type.</param>
    /// <param name="blobKeyPrefix">Prefix for generating the blob key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Blob key that identifies the stored image.</returns>
    public async Task<string> SaveProfileImageAsync(
        byte[] imageBytes,
        string contentType,
        string blobKeyPrefix,
        CancellationToken cancellationToken = default)
    {
        _ = contentType;

        using Image image = Image.Load(imageBytes);
        bool requiresResize = image.Width > 1024 || image.Height > 1024;

        image.Mutate(ctx =>
        {
            if (requiresResize)
            {
                ctx.Resize(
                    new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(1024, 1024),
                    });
            }
        });

        string blobKey = $"{blobKeyPrefix}-{Guid.CreateVersion7():N}.jpg";
        await using MemoryStream memoryStream = new();
        await image.SaveAsJpegAsync(memoryStream, new JpegEncoder { Quality = 85 }, cancellationToken);

        this.context.ImageBlobs.Add(
            new StoredImage
            {
                BlobKey = blobKey,
                Data = memoryStream.ToArray(),
                ContentType = "image/jpeg",
                CreatedAtUtc = DateTime.UtcNow,
                Sha256Hash = Convert.ToHexString(SHA256.HashData(memoryStream.ToArray())),
            });

        await this.context.SaveChangesAsync(cancellationToken);

        return blobKey;
    }

    /// <summary>
    ///     Reads a stored JPEG image by blob key.
    /// </summary>
    /// <param name="blobKey">Stored image blob key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Image bytes and content type when found; otherwise null.</returns>
    public async Task<(byte[] Content, string ContentType)?> ReadProfileImageAsync(
        string blobKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(blobKey) || blobKey.Contains("..", StringComparison.Ordinal))
        {
            return null;
        }

        StoredImage? image = await this.context.ImageBlobs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.BlobKey == blobKey, cancellationToken);

        return image == null ? null : (image.Data, image.ContentType);
    }
}