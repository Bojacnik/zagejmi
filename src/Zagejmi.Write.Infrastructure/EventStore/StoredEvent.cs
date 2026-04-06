using System;

namespace Zagejmi.Write.Infrastructure.EventStore;

/// <summary>
///     Represents an event stored in the application's primary event log table.
///     This is the long-term source of truth for the system.
/// </summary>
public class StoredEvent
{
    /// <summary>
    ///     Gets unique identifier for the stored event, typically generated as a new GUID when the event is created.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     Gets unique identifier of the aggregate to which this event belongs. This allows for grouping events by their
    ///     associated
    ///     aggregate, enabling the reconstruction of aggregate state by replaying events in order.
    /// </summary>
    public Guid AggregateId { get; init; }

    /// <summary>
    ///     Gets the type of the event, typically represented as a string that identifies the specific domain event (e.g.,
    ///     "UserCreated", "OrderPlaced"). This allows for deserialization and handling of events based on their type.
    /// </summary>
    public required string EventType { get; init; }

    /// <summary>
    ///     Gets the serialized data of the event, typically stored as a JSON string. This contains all the relevant
    ///     information about
    ///     the event, such as the properties of the domain event and their values at the time the event occurred. The data can
    ///     be deserialized back into a domain event object when needed for processing or replaying events to reconstruct
    ///     aggregate state.
    /// </summary>
    public required string Data { get; init; }

    /// <summary>
    ///     Gets the timestamp indicating when the event was created and stored in the event log. This is crucial for
    ///     maintaining the correct order of events, especially when reconstructing aggregate state by replaying events. The
    ///     timestamp allows the system to determine the sequence of events and ensure that they are processed in the correct
    ///     order, which is essential for maintaining consistency and correctness in an event-sourced system.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; }
}