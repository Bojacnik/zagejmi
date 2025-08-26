using Zagejmi.Server.Domain.Community.People;
using Zagejmi.Server.Domain.Entity;
using Zagejmi.Server.Domain.Events;

namespace Zagejmi.Server.Domain.Events.People;

public sealed record PersonDeleted : IDomainEvent<Person, Guid>
{
    public Guid AggregateId { get; }
    public Guid PersonId { get; }
    public DateTime Timestamp { get; }
    public EventTypeDomain EventType { get; }

    public PersonDeleted(Guid personId)
    {
        PersonId = personId;
        Timestamp = DateTime.UtcNow;
        EventType = EventTypeDomain.PersonDeleted;
    }
}