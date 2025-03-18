using Zagejmi.Domain.Events;

namespace Zagejmi.Application.Events.GoinTransactions;

public class GoinTransactionUpdatedEvent(
    Guid eventId,
    DateTime timestamp,
    ulong version,
    Guid aggregateId,
    string eventType)
    : IGoinTransactionEvent
{
    public Guid EventId { get; } = eventId;
    public DateTime Timestamp { get; } = timestamp;
    public ulong Version { get; } = version;
    public Guid AggregateId { get; } = aggregateId;
    public string EventType { get; } = eventType;
}