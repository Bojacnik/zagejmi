using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Write.Domain.Abstractions;

namespace Zagejmi.Write.Application.Abstractions;

/// <summary>
///     Interface for publishing domain events to an event bus, allowing for decoupled communication between different
///     parts of the system.
/// </summary>
public interface IEventBusProducer
{
    /// <summary>
    ///     Publishes a domain event message to the specified channel on the event bus.
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Task PublishAsync<T>(string channel, string message, CancellationToken cancellationToken = default)
        where T : IDomainEvent;
}