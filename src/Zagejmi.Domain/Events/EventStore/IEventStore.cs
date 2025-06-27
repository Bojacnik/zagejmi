using SharedKernel;

namespace Zagejmi.Domain.Events.EventStore;

public interface IEventStore
{
    Task SaveEventAsync<T>(T @event, CancellationToken cancellationToken) 
        where T : class, IDomainEvent;
}