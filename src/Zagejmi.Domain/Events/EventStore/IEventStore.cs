using SharedKernel;

namespace Zagejmi.Domain.Events.EventStore;

public interface IEventStore
{
    Task SaveEventAsync<TDomainEvent>(
        TDomainEvent @event,
        CancellationToken cancellationToken
    ) where TDomainEvent : IDomainEvent;
}