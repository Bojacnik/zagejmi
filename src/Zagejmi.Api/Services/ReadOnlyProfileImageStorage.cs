using System;
using System.Threading;
using System.Threading.Tasks;

namespace Zagejmi.Api.Services;

/// <summary>
///     Read-side image storage. Prefers read projections, then falls back to the shared image store.
/// </summary>
public sealed class ReadOnlyProfileImageStorage
{
    private readonly ZagejmiReadContext readContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ReadOnlyProfileImageStorage" /> class.
    /// </summary>
    public ReadOnlyProfileImageStorage(ZagejmiReadContext readContext)
    {
        this.readContext = readContext;
    }

    /// <summary>
    ///     Reads an existing profile image from disk or read DB.
    /// </summary>
    /// <returns>Image bytes and content type when found, otherwise null.</returns>
    public async Task<(byte[] Content, string ContentType)?> ReadProfileImageAsync(
        string blobKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(blobKey) || blobKey.Contains("..", StringComparison.Ordinal))
        {
            return null;
        }

        var profile = await this.readContext.UserProfiles
            .AsNoTracking()
            .Where(p => p.ProfileImageBlobKey == blobKey)
            .Select(p => new { p.ProfileImageData, p.ProfileImageContentType })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile != null && profile.ProfileImageData != null && profile.ProfileImageData.Length > 0)
        {
            string contentType = string.IsNullOrWhiteSpace(profile.ProfileImageContentType)
                ? "image/jpeg"
                : profile.ProfileImageContentType;
            return (profile.ProfileImageData, contentType);
        }

        var associate = await this.readContext.AssociateProfiles
            .AsNoTracking()
            .Where(p => p.BannerImageBlobKey == blobKey)
            .Select(p => new { p.BannerImageData, p.BannerImageContentType })
            .FirstOrDefaultAsync(cancellationToken);

        if (associate != null && associate.BannerImageData != null && associate.BannerImageData.Length > 0)
        {
            string contentType = string.IsNullOrWhiteSpace(associate.BannerImageContentType)
                ? "image/jpeg"
                : associate.BannerImageContentType;
            return (associate.BannerImageData, contentType);
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