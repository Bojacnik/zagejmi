using System;

namespace Zagejmi.Api;

/// <summary>
///     Persisted binary image payload stored in the write database.
/// </summary>
public sealed class StoredImage
{
    public Guid Id { get; init; }

    public required string BlobKey { get; init; }

    public required byte[] Data { get; init; }

    public required string ContentType { get; init; }

    public int Width { get; init; }

    public int Height { get; init; }

    public long SizeBytes { get; init; }

    public required string Sha256Hash { get; init; }

    public DateTime CreatedAtUtc { get; init; }
}