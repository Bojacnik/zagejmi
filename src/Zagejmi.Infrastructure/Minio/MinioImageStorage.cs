using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Minio;
using Minio.DataModel.Args;

using Zagejmi.Application.Images.Abstractions;

namespace Zagejmi.Infrastructure.Minio;

/// <summary>
///     Represents an implementation of the <see cref="IImageStorage" /> interface that uses MinIO for image storage.
/// </summary>
internal sealed class MinioImageStorage : IImageStorage
{
    private const string BucketName = "images";

    // The bucket only has to be created once per process; a fresh MinIO (e.g. a new Docker volume) has none.
    private static readonly SemaphoreSlim BucketLock = new(1, 1);
    private static bool bucketReady;

    private readonly IMinioClient client;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MinioImageStorage" /> class with the specified MinIO client.
    /// </summary>
    /// <param name="client">The MinIO client.</param>
    public MinioImageStorage(IMinioClient client)
    {
        this.client = client;
    }

    /// <summary>
    ///     Deletes an image from the MinIO storage with the specified key.
    /// </summary>
    /// <param name="key">The key of the image to be deleted.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task DeleteAsync(string key, CancellationToken cancellationToken)
    {
        await this.client.RemoveObjectAsync(
            new RemoveObjectArgs()
                .WithBucket(BucketName)
                .WithObject(key),
            cancellationToken);
    }

    /// <summary>
    ///     Gets a presigned URL for accessing an image stored in MinIO with the specified key and expiry time.
    /// </summary>
    /// <param name="key">The key of the image for which to generate a presigned URL.</param>
    /// <param name="expiry">The expiry time for the presigned URL.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The presigned URL for accessing the image.</returns>
    public async Task<string> GetAccessUrlAsync(
        string key,
        TimeSpan expiry,
        CancellationToken cancellationToken)
    {
        return await this.client.PresignedGetObjectAsync(
            new PresignedGetObjectArgs()
                .WithBucket(BucketName)
                .WithObject(key)
                .WithExpiry((int)expiry.TotalSeconds));
    }

    /// <summary>
    ///     Uploads an image to the MinIO storage with the specified key, content, and content type.
    /// </summary>
    /// <param name="key">The key for the image to be uploaded.</param>
    /// <param name="content">The content of the image to be uploaded.</param>
    /// <param name="contentType">The content type of the image to be uploaded.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The key of the uploaded image.</returns>
    /// <inheritdoc />
    public async Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken)
    {
        MemoryStream buffer = new();
        await this.client.GetObjectAsync(
            new GetObjectArgs()
                .WithBucket(BucketName)
                .WithObject(key)
                .WithCallbackStream((stream, ct) => stream.CopyToAsync(buffer, ct)),
            cancellationToken);

        buffer.Position = 0;
        return buffer;
    }

    public async Task<string> UploadAsync(
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken)
    {
        await this.EnsureBucketAsync(cancellationToken);

        await this.client.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(BucketName)
                .WithObject(key)
                .WithStreamData(content)
                .WithObjectSize(content.Length)
                .WithContentType(contentType),
            cancellationToken);

        return key;
    }

    private async Task EnsureBucketAsync(CancellationToken cancellationToken)
    {
        if (bucketReady)
        {
            return;
        }

        await BucketLock.WaitAsync(cancellationToken);
        try
        {
            if (!bucketReady)
            {
                bool exists = await this.client.BucketExistsAsync(
                    new BucketExistsArgs().WithBucket(BucketName),
                    cancellationToken);

                if (!exists)
                {
                    await this.client.MakeBucketAsync(new MakeBucketArgs().WithBucket(BucketName), cancellationToken);
                }

                bucketReady = true;
            }
        }
        finally
        {
            BucketLock.Release();
        }
    }
}
