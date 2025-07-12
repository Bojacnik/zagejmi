using SharedKernel;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.Domain.Community.User;

namespace Zagejmi.Domain.Events.GoinTransactions;

public record GoinTransactionCreatedEvent(
    DateTime Timestamp,
    EventTypeDomain EventType)
    : IGoinTransactionEvent<Person, Guid>
{
    public DateTime Timestamp { get; } = Timestamp;
    public EventTypeDomain EventType { get; } = EventType;

    public Person Apply(Person aggregate)
    {
        throw new NotImplementedException();
    }
}