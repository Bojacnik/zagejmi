namespace SharedKernel;

public interface IDomainEvent<TAggregateRoot, TAggregateId> 
    where TAggregateRoot : AggregateRoot<TAggregateRoot, TAggregateId>
    where TAggregateId : notnull
{
    DateTime Timestamp { get; }
    EventTypeDomain EventType { get; }

    public TAggregateRoot Apply(TAggregateRoot aggregate);
}