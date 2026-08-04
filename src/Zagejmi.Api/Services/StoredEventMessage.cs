using System;

namespace Zagejmi.Api.Services;

/// <summary>
///     Transport model for an event consumed by projection handlers.
/// </summary>
public sealed class StoredEventMessage
{
    /// <summary>
    ///     Gets or sets event identifier.
    /// </summary>
    public Guid EventId { get; set; }

    /// <summary>
    ///     Gets or sets aggregate identifier.
    /// </summary>
    public Guid AggregateId { get; set; }

    /// <summary>
    ///     Gets or sets event timestamp in UTC.
    /// </summary>
    public DateTime TimestampUtc { get; set; }

    /// <summary>
    ///     Gets or sets event type name.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets serialized event payload.
    /// </summary>
    public string Data { get; set; } = string.Empty;
}