using System;

namespace Zagejmi.Domain.Images;

/// <summary>
///     Application-level representation of a stored image blob, decoupled from persistence concerns.
/// </summary>
public sealed record ImageBlob
{
    /// <summary>
    ///     Gets the unique identifier for the image blob, typically generated as a GUID to ensure uniqueness across different
    ///     instances
    ///     and systems. This property serves as the primary key for the image blob entity and is essential for tracking and
    ///     managing image blobs effectively.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    ///     Gets the key used to reference the image blob in the underlying storage system (e.g., a cloud storage service).
    ///     This property is essential for locating and retrieving the image blob from storage, and it should be unique within
    ///     the storage context to prevent conflicts and ensure proper access to the correct image data.
    /// </summary>
    public required string BlobKey { get; init; }

    /// <summary>
    ///     Gets MIME type of the image blob, indicating the format of the image data (e.g., "image/jpeg", "image/png"). This
    ///     property is important for correctly interpreting and displaying the image, as well as for ensuring compatibility
    ///     with various image processing and rendering tools. The ContentType should adhere to standard MIME type conventions
    ///     to facilitate proper handling of the image data across different platforms and applications.
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    ///     Gets the size of the image blob in bytes, providing information about the amount of storage space occupied by the
    ///     image data.
    /// </summary>
    public required long SizeBytes { get; init; }

    /// <summary>
    ///     Gets the width of the image in pixels, indicating the horizontal dimension of the image. This property is important
    ///     for determining the image's aspect ratio and display dimensions.
    /// </summary>
    public required int Width { get; init; }

    /// <summary>
    ///     Gets the height of the image in pixels, indicating the vertical dimension of the image. This property is important
    ///     for determining the image's aspect ratio and display dimensions.
    /// </summary>
    public required int Height { get; init; }

    /// <summary>
    ///     Gets the timestamp indicating when the image blob was created, providing information about the age of the image
    ///     data and its relevance within the application context. This property is useful for tracking the lifecycle of the
    ///     image blob and for implementing features such as sorting, filtering, and archiving based on creation date.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    ///     Gets the SHA-256 hash of the image content, which serves as a unique fingerprint for the image data. This property
    ///     is essential for detecting duplicate images, verifying data integrity, and ensuring that the image content has not
    ///     been tampered with. The SHA-256 hash should be computed using a secure hashing algorithm and stored in a consistent
    ///     format (e.g., hexadecimal string) to facilitate comparison and validation across different systems and components.
    /// </summary>
    public required string Sha256Hash { get; init; }
}
