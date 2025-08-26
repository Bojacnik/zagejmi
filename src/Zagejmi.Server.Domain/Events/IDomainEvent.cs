using Zagejmi.Server.Domain.Entity;

namespace Zagejmi.Server.Domain.Events;

public interface IDomainEvent
{
    DateTime Timestamp { get; }
    EventTypeDomain EventType { get; }
}

public interface IDomainEvent<TAggregateRoot, TAggregateId> : IDomainEvent
    where TAggregateRoot : Aggregate<TAggregateRoot, TAggregateId>
    where TAggregateId : notnull
{
    TAggregateId AggregateId { get; }
}
