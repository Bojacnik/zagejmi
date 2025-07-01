using RabbitMQ.Client;
using SharedKernel;
using Zagejmi.Domain.Events.EventStore;
using Zagejmi.Infrastructure.EventBus;

namespace Zagejmi.Infrastructure.EventStore;

public class EventStore(IEventBusProducer producer) : IEventStore
{
    public async Task SaveEventAsync<TDomainEvent>(
        TDomainEvent @event,
        CancellationToken cancellationToken
    ) where TDomainEvent : IDomainEvent
    {
        CancellationTokenSource cts = new();
        await producer.SendAsync(@event, cts.Token);
    }
}