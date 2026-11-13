using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Zagejmi.Application.Images.Abstractions;

/// <summary>
///     Represents a contract for image storage operations, providing methods for uploading, deleting, and retrieving
///     access URLs for images. This interface defines the essential operations required for managing image storage in a
///     consistent manner, allowing for different implementations (e.g., cloud storage providers, local file systems) to be
///     used interchangeably. The IImageStorage interface ensures that image storage functionality can be abstracted and
///     integrated seamlessly into applications, promoting flexibility and maintainability in handling image assets.
/// </summary>
public interface IImageStorage
{
    /// <summary>
    ///     Uploads an image to the storage system and returns a unique key that can be used to reference the stored image. The
    ///     UploadAsync method accepts the image content as a stream, along with the desired content type (e.g., "image/jpeg",
    ///     "image/png") and a cancellation token to support asynchronous operation cancellation. The returned key can be used
    ///     for subsequent operations such as retrieving access URLs or deleting the image from storage. This method is
    ///     designed to handle various image formats and ensures that the uploaded images are stored securely and efficiently.
    /// </summary>
    /// <param name="key">The unique key for the image.</param>
    /// <param name="content">The stream containing the image content.</param>
    /// <param name="contentType">The content type of the image (e.g., "image/jpeg", "image/png").</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, returning the unique key for the uploaded image.</returns>
    Task<string> UploadAsync(
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Deletes an image from the storage system based on the provided unique key. The DeleteAsync method accepts the key
    ///     of the image to be deleted and a cancellation token to support asynchronous operation cancellation.
    /// </summary>
    /// <param name="key">The unique key for the image.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(
        string key,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Generates a temporary access URL for an image stored in the storage system, allowing clients to retrieve the image
    ///     without requiring direct access to the storage backend. The GetAccessUrlAsync method accepts the unique key of the
    ///     image, the desired expiry duration for the access URL, and a cancellation token to support asynchronous operation
    ///     cancellation. The returned URL is valid for the specified duration and can be used to access the image securely,
    ///     providing a convenient way to share or display images without exposing the underlying storage details.
    /// </summary>
    /// <param name="key">The unique key for the image.</param>
    /// <param name="expiry">The duration for which the access URL will be valid.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, returning the temporary access URL.</returns>
    Task<string> GetAccessUrlAsync(
        string key,
        TimeSpan expiry,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Opens an image for reading. The caller disposes the returned stream.
    /// </summary>
    /// <param name="key">The unique key for the image.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The image content.</returns>
    Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken);
}
