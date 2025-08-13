using MassTransit;
using Zagejmi.Server.Application.EventBus;
using Zagejmi.Server.Domain.Events;

namespace Zagejmi.Server.Infrastructure.EventBus;

public class EventBusProducerRabbitMq : IEventBusProducer
{
    public EventBusProducerRabbitMq(IBus bus)
    {
        _bus = bus;
    }

    public Task PublishAsync<T>(string queue, T message, CancellationToken cancellationToken = default)
        where T : IDomainEvent
    {
        return _bus.Publish(message, cancellationToken);
    }

    private readonly IBus _bus;
}