using Zagejmi.Domain.Entity;

namespace Zagejmi.Domain.Events;

public interface IPersonEvent<TAggregateRoot, TAggregateId>
    : IDomainEvent<TAggregateRoot, TAggregateId>
    where TAggregateRoot : AggregateRoot<TAggregateRoot, TAggregateId>
    where TAggregateId : notnull;