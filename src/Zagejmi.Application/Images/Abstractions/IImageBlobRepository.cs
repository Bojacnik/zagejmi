using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Domain.Images;

namespace Zagejmi.Application.Images.Abstractions;

/// <summary>
///     Provides persistence operations for image blobs.
/// </summary>
public interface IImageBlobRepository
{
    /// <summary>
    ///     Looks up an existing blob by its unique blob key.
    /// </summary>
    /// <param name="blobKey">The unique blob key of the image blob to look up.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The image blob if found; otherwise, null.</returns>
    Task<ImageBlob?> GetByBlobKeyAsync(string blobKey, CancellationToken ct = default);

    /// <summary>
    ///     Looks up existing blobs by a prefix of their blob keys, returning all matching blobs.
    /// </summary>
    /// <param name="prefix">The prefix to match blob keys against.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A list of image blobs whose keys start with the specified prefix.</returns>
    Task<IReadOnlyList<ImageBlob>> GetByBlobKeyPrefixAsync(string prefix, CancellationToken ct = default);

    /// <summary>
    ///     Looks up an existing blob by its unique identifier.
    /// </summary>
    /// <param name="id">Identifier of the blob to look up.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Blob record if found, otherwise null.</returns>
    Task<ImageBlob?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    ///     Looks up an existing blob by content hash, used to avoid storing duplicate images.
    /// </summary>
    /// <param name="sha256Hash">SHA-256 hash of the image content.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Blob record if found, otherwise null.</returns>
    Task<ImageBlob?> GetBySha256HashAsync(string sha256Hash, CancellationToken ct = default);

    /// <summary>
    ///     Adds a new image blob record to the repository.
    /// </summary>
    /// <param name="record">The image blob record to add.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The task representing the asynchronous operation.</returns>
    Task AddAsync(ImageBlob record, CancellationToken ct = default);

    /// <summary>
    ///     Deletes an existing image blob record from the repository by its unique identifier.
    /// </summary>
    /// <param name="id">Identifier of the blob to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The task representing the asynchronous operation.</returns>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}