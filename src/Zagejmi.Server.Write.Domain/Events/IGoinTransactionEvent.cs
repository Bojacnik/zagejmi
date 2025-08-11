using Zagejmi.Server.Write.Domain.Entity;

namespace Zagejmi.Server.Write.Domain.Events;

public interface IGoinTransactionEvent<TAggregateRoot, TAggregateId> : IDomainEvent<TAggregateRoot, TAggregateId>
    where TAggregateRoot : AggregateRoot<TAggregateRoot, TAggregateId>
    where TAggregateId : notnull;