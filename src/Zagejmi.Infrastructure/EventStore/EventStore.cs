using LanguageExt;
using Zagejmi.Application.EventStore;
using Zagejmi.Domain;
using Zagejmi.Domain.Entity;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Infrastructure.EventStore;

public class EventStore<TAggregateRoot, TAggregateRootId>(
    IEventBusProducer<TAggregateRoot, TAggregateRootId> producer
)
    : IEventStore<TAggregateRoot, TAggregateRootId>
    where TAggregateRoot : AggregateRoot<TAggregateRoot, TAggregateRootId>
    where TAggregateRootId : notnull
{
    public async Task<Either<FailureEventStore, Unit>> SaveEventAsync<TDomainEvent>(
        TDomainEvent @event, CancellationToken cancellationToken)
        where TDomainEvent : IDomainEvent<TAggregateRoot, TAggregateRootId>
    {
        CancellationTokenSource cts = new();
        await producer.SendAsync(@event, cts.Token);
        return Unit.Default;
    }
}