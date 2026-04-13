using System;

namespace Zagejmi.Write.Infrastructure.EventStore;

public sealed class StoredEventMessage
{
    public Guid EventId { get; set; }

    public Guid AggregateId { get; set; }

    public DateTime TimestampUtc { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string Data { get; set; } = string.Empty;
}