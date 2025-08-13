using LanguageExt;
using Zagejmi.Server.Domain.Entity;
using Zagejmi.Server.Domain.Events;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Server.Application.EventStore;

public interface IEventStore<TAggregateRoot, TAggregateRootId>
    where TAggregateRoot : AggregateRoot<TAggregateRoot, TAggregateRootId>
    where TAggregateRootId : notnull
{
    Task<Either<FailureEventStore, Unit>> SaveEventAsync<TDomainEvent>(TDomainEvent @event,
        CancellationToken cancellationToken = default) where TDomainEvent : IDomainEvent<TAggregateRoot, TAggregateRootId>;
}