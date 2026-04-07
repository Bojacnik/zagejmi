using System;

namespace Zagejmi.Write.Infrastructure;

/// <summary>
///     Represents an event stored in the application's primary event log table.
///     This is the long-term source of truth for the system.
/// </summary>
public class StoredEvent
{
    /// <summary>
    ///     Gets the unique identifier for this stored event. This is the primary key for the event log table and is
    ///     used to uniquely identify each event. It is typically generated as a new GUID when the event is created and stored
    ///     in the database. This property is essential for maintaining the integrity of the event log and ensuring that each
    ///     event can be uniquely referenced and retrieved when needed. It is also used for tracking and auditing purposes,
    ///     allowing the system to maintain a complete history of all events that have occurred within the application.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     Gets the unique identifier of the aggregate root that this event is associated with. This property is used
    ///     to
    ///     link the event to a specific aggregate in the domain model, allowing for efficient retrieval of all events related
    ///     to a particular aggregate when reconstructing its state. The AggregateId is crucial for maintaining the consistency
    ///     of the event log and ensuring that events can be correctly associated with their corresponding aggregates, enabling
    ///     the system to rebuild the state of an aggregate by replaying its events in the correct order.
    /// </summary>
    public Guid AggregateId { get; init; }

    /// <summary>
    ///     Gets the type of the event. This is typically a string that represents the name of the event class or a
    ///     unique identifier for the event type. The EventType property is used to identify the specific type of event that
    ///     occurred, allowing the system to determine how to process and handle the event when it is retrieved from the event
    ///     log. It is essential for deserializing the event data back into the appropriate event class when reconstructing the
    ///     state of an aggregate or when processing events for other purposes, such as publishing to an event bus or
    ///     triggering side effects in the application. By storing the event type as a string, the system can maintain
    ///     flexibility in handling different types of events and can easily extend the event log to accommodate new event
    ///     types as the application evolves without requiring changes to the underlying database schema.
    /// </summary>
    public required string EventType { get; init; }

    /// <summary>
    ///     Gets the serialized data of the event. This property contains the actual content of the event, typically
    ///     stored as a JSON string or another serialized format that represents the state changes or information associated
    ///     with the event. The Data property is crucial for capturing the details of the event, allowing the
    ///     system to reconstruct the event object when retrieving it from the event log. It is important to ensure that the
    ///     data is properly serialized and deserialized to maintain the integrity of the event information and     enable
    ///     accurate reconstruction of the event's state when processing it for various purposes, such as rebuilding aggregate
    ///     state, publishing to an event bus, or triggering side effects in the application. By storing the event data as a
    ///     serialized string, the system can maintain flexibility in handling different types of events and can easily extend
    ///     the event log to accommodate new event types and data structures as the application evolves without requiring
    ///     changes to the underlying database schema.
    /// </summary>
    public required string Data { get; init; }

    /// <summary>
    ///     Gets the timestamp in UTC when the event occurred. This property is essential for maintaining the chronological
    ///     order of events in the event log, allowing the system to accurately reconstruct the state of an aggregate by
    ///     replaying its events in the correct sequence. All timestamps are stored in UTC to ensure consistency and eliminate
    ///     timezone confusion. It is important to ensure that the timestamp is accurate and consistent across all events to
    ///     maintain the integrity of the event log and ensure that events can be correctly ordered and processed based on
    ///     their
    ///     occurrence time. By storing the timestamp of each event in UTC, the system can enable features such as auditing,
    ///     monitoring, and debugging by providing a clear timeline of when events occurred.
    /// </summary>
    public DateTime Timestamp { get; init; }
}