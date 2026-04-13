using System;

namespace Zagejmi.Read.Api.Models;

/// <summary>
///     Persisted event record used for rebuilding read projections.
/// </summary>
public sealed class StoredEventReadModel
{
    /// <summary>
    ///     Gets or sets the event identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the aggregate identifier.
    /// </summary>
    public Guid AggregateId { get; set; }

    /// <summary>
    ///     Gets or sets the event timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    ///     Gets or sets the event type name.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the serialized event payload.
    /// </summary>
    public string Data { get; set; } = string.Empty;
}