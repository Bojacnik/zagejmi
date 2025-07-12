using SharedKernel;

namespace Zagejmi.Domain.Events;

public interface IGoinTransactionEvent<TAggregateRoot, TAggregateId> : IDomainEvent<TAggregateRoot, TAggregateId>
    where TAggregateRoot : AggregateRoot<TAggregateRoot, TAggregateId>
    where TAggregateId : notnull;