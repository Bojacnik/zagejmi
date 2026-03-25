using Zagejmi.Write.Domain.Events;

namespace Zagejmi.Write.Application.EventBus;

public interface IEventBusProducer
{
    public Task PublishAsync<T>(string channel, T message, CancellationToken cancellationToken = default)
        where T : IDomainEvent;
}