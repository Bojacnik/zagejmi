using Zagejmi.Domain.Events;

namespace Zagejmi.Domain;

public abstract class Entity<TDomainEvent> : AggregateRoot<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
}