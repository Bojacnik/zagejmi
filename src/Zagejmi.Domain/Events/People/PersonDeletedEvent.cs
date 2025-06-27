namespace Zagejmi.Domain.Events.People;

public class PersonDeletedEvent(
    Guid eventId,
    DateTime timestamp,
    ulong version,
    Guid aggregateId,
    string eventType) : IPersonEvent
{
    public Guid EventId { get; } = eventId;
    public DateTime Timestamp { get; } = timestamp;
    public ulong Version { get; } = version;
    public Guid AggregateId { get; } = aggregateId;
    public string EventType { get; } = eventType;
    
    // TODO: fix me
}