using System.Threading;
using System.Threading.Tasks;

namespace Zagejmi.Application.Abstractions;

/// <summary>
///     Write-side storage contract for image persistence.
/// </summary>
public interface IProfileImageStorage
{
    /// <summary>
    ///     Persists an uploaded image and returns its blob key.
    /// </summary>
    /// <param name="imageBytes">Raw image bytes.</param>
    /// <param name="contentType">Original content type of the uploaded image.</param>
    /// <param name="blobKeyPrefix">Blob key prefix that identifies the image category.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Opaque blob key used to reference the stored image.</returns>
    Task<string> SaveProfileImageAsync(
        byte[] imageBytes,
        string contentType,
        string blobKeyPrefix,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads a previously persisted image by blob key.
    /// </summary>
    /// <param name="blobKey">Image blob key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Image payload and content type, or null when the key does not exist.</returns>
    Task<(byte[] Content, string ContentType)?> ReadProfileImageAsync(
        string blobKey,
        CancellationToken cancellationToken = default);
}