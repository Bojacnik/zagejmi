using LanguageExt;
using Zagejmi.Domain;
using Zagejmi.Domain.Entity;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Application.EventStore;

public interface IEventStore<TAggregateRoot, TAggregateRootId>
    where TAggregateRoot : AggregateRoot<TAggregateRoot, TAggregateRootId>
    where TAggregateRootId : notnull
{
    Task<Either<FailureEventStore, Unit>> SaveEventAsync<TDomainEvent>(TDomainEvent @event,
        CancellationToken cancellationToken = default) where TDomainEvent : IDomainEvent<TAggregateRoot, TAggregateRootId>;
}