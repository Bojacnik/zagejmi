// Zagejmi.Infrastructure/Images/Repositories/ImageBlobRepository.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Zagejmi.Application.Images.Abstractions;
using Zagejmi.Domain.Images;
using Zagejmi.Infrastructure.Images.Ctx;
using Zagejmi.Infrastructure.Images.Records;

namespace Zagejmi.Infrastructure.Images.Repositories;

/// <summary>
///     Implements the <see cref="IImageBlobRepository" /> interface, providing persistence operations for image blobs
///     using Entity Framework Core and a relational database context. This repository encapsulates the data access logic
///     for image blobs, allowing for retrieval, addition, and deletion of image blob records in a structured and efficient
///     manner. It ensures that the application can manage image blobs effectively while maintaining separation of concerns
///     between the domain model and the persistence layer.
/// </summary>
public sealed class ImageBlobRepository : IImageBlobRepository
{
    private readonly IDbContextFactory<BlobImagesContext> contextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ImageBlobRepository" /> class with the specified context factory.
    /// </summary>
    /// <param name="contextFactory">Factory for <see cref="BlobImagesContext" /> instances.</param>
    public ImageBlobRepository(IDbContextFactory<BlobImagesContext> contextFactory)
    {
        this.contextFactory = contextFactory;
    }

    /// <summary>
    ///     Looks up an existing blob by its unique blob key. If found, it returns the corresponding <see cref="ImageBlob" />.
    /// </summary>
    /// <param name="blobKey">The unique blob key of the image blob to look up.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The image blob if found; otherwise, null.</returns>
    public async Task<ImageBlob?> GetByBlobKeyAsync(string blobKey, CancellationToken ct = default)
    {
        await using BlobImagesContext context = await this.contextFactory.CreateDbContextAsync(ct);

        ImageBlobRecord? entity = await context.ImageBlobs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.BlobKey == blobKey, ct);

        return entity is null ? null : ToModel(entity);
    }

    /// <summary>
    ///     Looks up existing blobs by a prefix of their blob keys, returning all matching blobs. This method retrieves all
    ///     image blobs whose blob keys start with the specified prefix, allowing for efficient filtering and retrieval of
    ///     related image blobs based on naming conventions or organizational structures. It is useful for scenarios where
    ///     image blobs are grouped or categorized by a common prefix in their keys.
    /// </summary>
    /// <param name="prefix">The prefix to match against blob keys.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A list of image blobs matching the prefix.</returns>
    public async Task<IReadOnlyList<ImageBlob>> GetByBlobKeyPrefixAsync(string prefix, CancellationToken ct = default)
    {
        await using BlobImagesContext context = await this.contextFactory.CreateDbContextAsync(ct);

        List<ImageBlobRecord> entities = await context.ImageBlobs
            .AsNoTracking()
            .Where(x => EF.Functions.Like(x.BlobKey, prefix + "%"))
            .ToListAsync(ct);

        return entities.Select(ToModel).ToList();
    }

    /// <summary>
    ///     Looks up an existing image blob by its unique identifier. If found, it returns the corresponding
    ///     <see cref="ImageBlob" />.
    /// </summary>
    /// <param name="id">Identifier of the image blob to look up.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The image blob if found; otherwise, null.</returns>
    public async Task<ImageBlob?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using BlobImagesContext context = await this.contextFactory.CreateDbContextAsync(ct);

        ImageBlobRecord? entity = await context.ImageBlobs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return entity is null ? null : ToModel(entity);
    }

    /// <summary>
    ///     Looks up an existing image blob by its SHA-256 hash, which is used to avoid storing duplicate images. If found, it
    ///     returns the corresponding <see cref="ImageBlob" />.
    /// </summary>
    /// <param name="sha256Hash">The SHA-256 hash of the image blob to look up.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The image blob if found; otherwise, null.</returns>
    public async Task<ImageBlob?> GetBySha256HashAsync(string sha256Hash, CancellationToken ct = default)
    {
        await using BlobImagesContext context = await this.contextFactory.CreateDbContextAsync(ct);

        ImageBlobRecord? entity = await context.ImageBlobs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Sha256Hash == sha256Hash, ct);

        return entity is null ? null : ToModel(entity);
    }

    /// <summary>
    ///     Adds a new image blob record to the repository. This method creates a new <see cref="ImageBlobRecord" /> entity
    ///     from the provided <see cref="ImageBlob" /> and persists it to the database. It ensures that the image blob data is
    ///     stored in a structured and efficient manner, allowing for future retrieval and management of the image blob
    ///     records.
    /// </summary>
    /// <param name="blob">Image blob to add to the repository.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The task representing the asynchronous operation.</returns>
    public async Task AddAsync(ImageBlob blob, CancellationToken ct = default)
    {
        await using BlobImagesContext context = await this.contextFactory.CreateDbContextAsync(ct);

        ImageBlobRecord entity = new()
        {
            Id = blob.Id,
            BlobKey = blob.BlobKey,
            ContentType = blob.ContentType,
            SizeBytes = blob.SizeBytes,
            Width = blob.Width,
            Height = blob.Height,
            CreatedAt = blob.CreatedAt,
            Sha256Hash = blob.Sha256Hash,
        };

        await context.ImageBlobs.AddAsync(entity, ct);
        await context.SaveChangesAsync(ct);
    }

    /// <summary>
    ///     Deletes an existing image blob from the repository.
    /// </summary>
    /// <param name="id">Identifier of the image blob to delete.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The task representing the asynchronous operation.</returns>
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await using BlobImagesContext context = await this.contextFactory.CreateDbContextAsync(ct);

        ImageBlobRecord? entity = await context.ImageBlobs.FindAsync([id], ct);
        if (entity is null)
        {
            return;
        }

        context.ImageBlobs.Remove(entity);
        await context.SaveChangesAsync(ct);
    }

    private static ImageBlob ToModel(ImageBlobRecord entity)
    {
        return new ImageBlob
        {
            Id = entity.Id,
            BlobKey = entity.BlobKey,
            ContentType = entity.ContentType,
            SizeBytes = entity.SizeBytes,
            Width = entity.Width,
            Height = entity.Height,
            CreatedAt = entity.CreatedAt,
            Sha256Hash = entity.Sha256Hash,
        };
    }
}
