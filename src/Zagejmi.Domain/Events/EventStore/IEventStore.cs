using LanguageExt;
using SharedKernel;
using SharedKernel.Failures;

namespace Zagejmi.Domain.Events.EventStore;

public interface IEventStore<TAggregateRoot, TAggregateRootId>
    where TAggregateRoot : AggregateRoot<TAggregateRoot, TAggregateRootId>
    where TAggregateRootId : notnull
{
    Task<Either<FailureEventStore, Unit>> SaveEventAsync<TDomainEvent>(TDomainEvent @event,
        CancellationToken cancellationToken = default) where TDomainEvent : IDomainEvent<TAggregateRoot, TAggregateRootId>;
}