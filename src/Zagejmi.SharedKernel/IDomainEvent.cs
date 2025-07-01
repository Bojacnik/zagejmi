namespace SharedKernel;

public interface IDomainEvent
{
    DateTime Timestamp { get; }
    string EventType { get; }
}