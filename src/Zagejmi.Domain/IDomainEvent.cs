using Zagejmi.Domain.Entity;

namespace Zagejmi.Domain;

public interface IDomainEvent
{
    DateTime Timestamp { get; }
    EventTypeDomain EventType { get; }
}

public interface IDomainEvent<TAggregateRoot, TAggregateId> : IDomainEvent
    where TAggregateRoot : AggregateRoot<TAggregateRoot, TAggregateId>
    where TAggregateId : notnull
{
    public TAggregateRoot Apply(TAggregateRoot aggregate);
}