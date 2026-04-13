using System;

namespace Zagejmi.Read.Api.Models;

/// <summary>
///     Stores the last processed timestamp for a projection worker.
/// </summary>
public sealed class ProjectionCheckpoint
{
    /// <summary>
    ///     Gets or sets the unique projector identifier.
    /// </summary>
    public string ProjectorName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the last processed event timestamp in UTC.
    /// </summary>
    public DateTime? LastProcessedAtUtc { get; set; }
}