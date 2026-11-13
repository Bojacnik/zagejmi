using System;
using System.Threading;
using System.Threading.Tasks;

namespace Zagejmi.Application.Images.Abstractions;

/// <summary>
///     Write-side storage contract for image persistence.
/// </summary>
public interface IProfileImageStorage
{
    /// <summary>
    ///     Uploads an image and returns the stored blob key.
    /// </summary>
    /// <param name="imageBytes">Raw uploaded image bytes.</param>
    /// <param name="imageName">Logical image name, e.g. avatar or banner.</param>
    /// <param name="ownerId">Owner identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Blob key used to reference the stored image.</returns>
    Task<string> UploadAsync(
        byte[] imageBytes,
        string imageName,
        Guid ownerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Generates a temporary URL granting access to an image.
    /// </summary>
    /// <param name="blobKey">Stored image blob key.</param>
    /// <param name="expiry">How long the URL remains valid.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Temporary image URL.</returns>
    Task<string?> GenerateAccessUrlAsync(
        string blobKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes an image from storage.
    /// </summary>
    /// <param name="blobKey">Stored image blob key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the job.</returns>
    Task DeleteAsync(
        string blobKey,
        CancellationToken cancellationToken = default);
}