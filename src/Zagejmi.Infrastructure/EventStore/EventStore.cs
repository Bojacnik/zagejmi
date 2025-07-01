using RabbitMQ.Client;
using SharedKernel;
using Zagejmi.Domain.Events.EventStore;
using Zagejmi.Infrastructure.EventBus;

namespace Zagejmi.Infrastructure.EventStore;

public class EventStore(IConnection connection) : IEventStore
{
    public async Task SaveEventAsync<T>(
        T @event,
        CancellationToken cancellationToken
    ) where T : class, IDomainEvent
    {
        await RabbitMqProducer.SendMessage(">" + @event.GetType().Name + "", @event.EventType, connection);
    }
}