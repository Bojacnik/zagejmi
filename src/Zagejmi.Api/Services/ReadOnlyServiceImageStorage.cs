using System;
using System.Threading;
using System.Threading.Tasks;

namespace Zagejmi.Api.Services;

/// <summary>
///     Read-side image storage used for service images. Prefers read projections, then falls back to the shared image
///     store.
/// </summary>
public sealed class ReadOnlyServiceImageStorage
{
    private readonly ZagejmiReadContext readContext;

    public ReadOnlyServiceImageStorage(ZagejmiReadContext readContext)
    {
        this.readContext = readContext;
    }

    public async Task<(byte[] Content, string ContentType)?> ReadServiceImageAsync(
        string blobKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(blobKey) || blobKey.Contains("..", StringComparison.Ordinal))
        {
            return null;
        }

        var svc = await this.readContext.Services
            .AsNoTracking()
            .Where(s => s.ImageBlobKey == blobKey)
            .Select(s => new { s.ImageData, s.ImageContentType })
            .FirstOrDefaultAsync(cancellationToken);

        if (svc != null && svc.ImageData != null && svc.ImageData.Length > 0)
        {
            string contentType = string.IsNullOrWhiteSpace(svc.ImageContentType) ? "image/jpeg" : svc.ImageContentType;
            return (svc.ImageData, contentType);
        }

        StoredImage? image = await this.readContext.ImageBlobs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.BlobKey == blobKey, cancellationToken);

        if (image != null)
        {
            return (image.Data, image.ContentType);
        }

        return null;
    }
}