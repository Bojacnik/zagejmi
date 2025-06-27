namespace SharedKernel;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime Timestamp { get; }
    ulong Version { get; }
    Guid AggregateId { get; }
    string EventType { get; }
}