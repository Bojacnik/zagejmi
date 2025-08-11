using LanguageExt;
using Zagejmi.Server.Write.Domain;
using Zagejmi.Server.Write.Domain.Entity;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Server.Write.Application.EventStore;

public interface IEventStore<TAggregateRoot, TAggregateRootId>
    where TAggregateRoot : AggregateRoot<TAggregateRoot, TAggregateRootId>
    where TAggregateRootId : notnull
{
    Task<Either<FailureEventStore, Unit>> SaveEventAsync<TDomainEvent>(TDomainEvent @event,
        CancellationToken cancellationToken = default) where TDomainEvent : IDomainEvent<TAggregateRoot, TAggregateRootId>;
}