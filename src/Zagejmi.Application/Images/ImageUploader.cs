using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Images.Abstractions;
using Zagejmi.Domain.Images;
using Zagejmi.Shared;

namespace Zagejmi.Application.Images;

/// <summary>
///     Processes an uploaded image, stores it and records its metadata. Identical images are stored only once.
/// </summary>
public sealed class ImageUploader
{
    /// <summary>
    ///     The largest upload accepted, before processing.
    /// </summary>
    public const long MaxUploadBytes = 10 * 1024 * 1024;

    private readonly IImageProcessor imageProcessor;
    private readonly IImageStorage imageStorage;
    private readonly IImageBlobRepository imageBlobRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ImageUploader" /> class.
    /// </summary>
    /// <param name="imageProcessor">Validates and normalizes images.</param>
    /// <param name="imageStorage">Stores image files.</param>
    /// <param name="imageBlobRepository">Stores image metadata.</param>
    public ImageUploader(
        IImageProcessor imageProcessor,
        IImageStorage imageStorage,
        IImageBlobRepository imageBlobRepository)
    {
        this.imageProcessor = imageProcessor;
        this.imageStorage = imageStorage;
        this.imageBlobRepository = imageBlobRepository;
    }

    /// <summary>
    ///     Processes and stores an image.
    /// </summary>
    /// <param name="content">The uploaded file.</param>
    /// <param name="limits">The largest allowed size after processing.</param>
    /// <param name="keyPrefix">Storage folder, e.g. <c>profiles/{profileId}</c>.</param>
    /// <param name="crop">The part of the image chosen by the user, if any.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The ID of the stored image, or a validation failure.</returns>
    public async Task<Result<Guid>> UploadAsync(
        Stream content,
        ImageLimits limits,
        string keyPrefix,
        CropRegion? crop = null,
        CancellationToken cancellationToken = default)
    {
        if (content.CanSeek && content.Length > MaxUploadBytes)
        {
            return Result<Guid>.Failure(ErrorDetail.Validation(
                "Image.TooLarge",
                "Obrázek může mít nejvýše 10 MB."));
        }

        Result<ProcessedImage> processed = await this.imageProcessor.ProcessAsync(content, limits, crop, cancellationToken);
        if (processed.IsFailure)
        {
            return Result<Guid>.Failure(processed.Error!);
        }

        ProcessedImage image = processed.Value;
        string hash = Convert.ToHexStringLower(SHA256.HashData(image.Content));

        ImageBlob? existing = await this.imageBlobRepository.GetBySha256HashAsync(hash, cancellationToken);
        if (existing is not null)
        {
            return existing.Id;
        }

        Guid id = Guid.CreateVersion7();
        string key = $"{keyPrefix}/{id}.{image.Extension}";

        using (MemoryStream upload = new(image.Content, writable: false))
        {
            await this.imageStorage.UploadAsync(key, upload, image.ContentType, cancellationToken);
        }

        await this.imageBlobRepository.AddAsync(
            new ImageBlob
            {
                Id = id,
                BlobKey = key,
                ContentType = image.ContentType,
                SizeBytes = image.Content.LongLength,
                Width = image.Width,
                Height = image.Height,
                CreatedAt = DateTimeOffset.UtcNow,
                Sha256Hash = hash,
            },
            cancellationToken);

        return id;
    }
}
