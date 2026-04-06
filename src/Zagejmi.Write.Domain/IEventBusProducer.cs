using System.Threading;
using System.Threading.Tasks;

using LanguageExt;

using Zagejmi.Shared.Failures;
using Zagejmi.Write.Domain.Abstractions;

namespace Zagejmi.Write.Domain;

/// <summary>
///     Defines the contract for an event bus producer, responsible for sending domain events to an event bus or message
///     broker. This interface abstracts the underlying implementation of the event bus, allowing for flexibility in
///     how events are published and consumed within the system. The producer is responsible for ensuring that domain
///     events are reliably sent to the event bus, enabling other components or services to react to these events and
///     maintain eventual consistency across the system. The use of the Either type from LanguageExt allows for handling
///     potential failures in the event sending process, providing a way to capture and manage errors that may occur during
///     the communication with the event bus. The SendAsync method is asynchronous, allowing for non-blocking operations
///     when sending events, which is crucial for maintaining the responsiveness and scalability of the application.
///     Overall, this interface plays a critical role in the event-driven architecture of the system, facilitating the
///     decoupling of components and enabling the flow of information through domain events.
/// </summary>
public interface IEventBusProducer
{
    /// <summary>
    ///     Asynchronously sends a domain event to the event bus or message broker. This method takes an instance of
    ///     IDomainEvent, which represents the event to be published, and a CancellationToken to allow for cancellation of the
    ///     operation if needed. The method returns a Task that resolves to an Either type, which can contain either a Failure
    ///     object in case of an error during the sending process or a Unit value indicating successful completion. The
    ///     implementation of this method is responsible for handling the communication with the event bus, ensuring that the
    ///     event is properly serialized and sent to the correct topic or queue, and managing any potential errors that may
    ///     arise during this process. The asynchronous nature of the method allows for efficient handling of event publishing
    ///     without blocking the calling thread, which is essential for maintaining the performance and scalability of the
    ///     application. Overall, this method is a key part of the event-driven architecture, enabling the flow of domain
    ///     events throughout the system and allowing other components to react to these events in a decoupled manner.
    /// </summary>
    /// <param name="event">
    ///     Event to be sent to the event bus, represented as an instance of IDomainEvent. This parameter
    ///     contains all the relevant information about the domain event that needs to be published, such as its type and
    ///     properties. The implementation of the SendAsync method will be responsible for serializing this event and sending
    ///     it to the appropriate topic or queue in the event bus or message broker. The event should be properly structured
    ///     and contain all necessary information to allow for successful processing by the consumers of the event.
    /// </param>
    /// <param name="cancellationToken">
    ///     A CancellationToken that can be used to cancel the asynchronous operation of sending the event. This allows for
    ///     graceful handling of cancellation requests, such as when the application is shutting down or when the operation is
    ///     taking too long. The implementation of the SendAsync method should respect this cancellation token and ensure that
    ///     any ongoing operations are properly terminated if a cancellation request is received. This is important for
    ///     maintaining the responsiveness and stability of the application, especially in scenarios where network issues or
    ///     other factors may cause delays in sending events to the event bus. By using a CancellationToken, the system can
    ///     avoid unnecessary resource consumption and ensure that it can respond to cancellation requests in a timely manner.
    /// </param>
    /// <returns>
    ///     A Task that resolves to an Either type, which can contain either a Failure object in case of an error during the
    ///     sending process or a Unit value indicating successful completion. The Either type allows for handling potential
    ///     failures in a functional programming style, providing a way to capture and manage errors that may occur during the
    ///     communication with the event bus. If the sending process is successful, the Task will resolve to a Unit value,
    ///     indicating that the operation completed without any issues. If there is an error during the sending process, such
    ///     as a network failure or an issue with the event bus, the Task will resolve to a Failure object, which can contain
    ///     details about the error that occurred. This allows the calling code to handle the failure case appropriately, such
    ///     as by logging the error, retrying the operation, or taking other corrective actions as needed. Overall, the return
    ///     type of this method provides a robust way to manage the outcomes of the event sending process, ensuring that both
    ///     success and failure cases are properly handled in the application.
    /// </returns>
    public Task<Either<Failure, Unit>> SendAsync(
        IDomainEvent @event,
        CancellationToken cancellationToken);
}