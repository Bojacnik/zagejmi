namespace SharedKernel;

public abstract class Entity<TDomainEvent> : AggregateRoot<TDomainEvent> where TDomainEvent : IDomainEvent
{
}