using System;
using System.ComponentModel.DataAnnotations;

namespace Zagejmi.Infrastructure.Images.Records;

/// <summary>
///     Represents a record for an image blob stored in the system, containing metadata such as the blob key, content type,
///     and dimensions.
/// </summary>
public sealed class ImageBlobRecord
{
    /// <summary>
    ///     Gets the unique identifier for the image blob record.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     Gets the unique key for the blob in the storage system. This key is used to retrieve the blob from the
    ///     storage system.
    /// </summary>
    [MaxLength(256)]
    public required string BlobKey { get; init; }

    /// <summary>
    ///     Gets the content type (MIME type) of the image blob, indicating the format of the image
    ///     (e.g., "image/png", "image/jpeg").
    /// </summary>
    [MaxLength(128)]
    public required string ContentType { get; init; }

    /// <summary>
    ///     Gets the size of the image blob in bytes. This property indicates the total number of bytes used to store
    ///     the image blob in the storage system.
    /// </summary>
    [Range(0, long.MaxValue)]
    public long SizeBytes { get; init; }

    /// <summary>
    ///     Gets the width of the image in pixels. This property indicates the horizontal dimension of the image.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int Width { get; init; }

    /// <summary>
    ///     Gets the height of the image in pixels. This property indicates the vertical dimension of the image.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int Height { get; init; }

    /// <summary>
    ///     Gets the timestamp indicating when the image blob record was created. This property is useful for tracking
    ///     the creation time of the image blob and can be used for auditing or sorting purposes.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    ///     Gets the SHA-256 hash of the image blob. This property is used to verify the integrity of the image data
    ///     and can be used for deduplication or caching purposes. The hash is computed based on the content of the image blob.
    /// </summary>
    [MaxLength(64)]
    public required string Sha256Hash { get; init; }
}