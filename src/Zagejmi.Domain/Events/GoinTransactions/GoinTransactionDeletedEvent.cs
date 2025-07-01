namespace Zagejmi.Domain.Events.GoinTransactions;

public class GoinTransactionDeletedEvent(
    DateTime timestamp,
    string eventType)
    : IGoinTransactionEvent
{
    public DateTime Timestamp { get; } = timestamp;
    public string EventType { get; } = eventType;
}