using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Zagejmi.Write.Infrastructure.EventStore;

public interface IStoredEventPublisher
{
    Task PublishAsync(
        IReadOnlyCollection<Infrastructure.StoredEvent> events,
        CancellationToken cancellationToken = default);
}