using Zagejmi.Domain.Events;

namespace Zagejmi.Application.EventStore;

public interface IEventStore
{
    Task SaveEventAsync<T>(T @event, CancellationToken cancellationToken) 
        where T : class, IDomainEvent;
}