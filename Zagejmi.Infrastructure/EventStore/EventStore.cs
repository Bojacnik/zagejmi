using Zagejmi.Application.EventStore;
using Zagejmi.Domain.Events;

namespace Zagejmi.Infrastructure.EventStore;

public class EventStore(Microsoft.EntityFrameworkCore.DbContext context) : IEventStore
{
    public async Task SaveEventAsync<T>(
        T @event,
        CancellationToken cancellationToken
    ) where T : class, IDomainEvent
    {
        await context.Set<T>().AddAsync(@event, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}