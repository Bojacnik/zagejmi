using Zagejmi.Server.Domain.Events;

namespace Zagejmi.Server.Application.EventBus;

public interface IEventBusProducer
{
    public Task PublishAsync<T>(string channel, T message, CancellationToken cancellationToken = default)
        where T : IDomainEvent;
}