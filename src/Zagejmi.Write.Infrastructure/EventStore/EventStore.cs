using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Zagejmi.Write.Application.Abstractions;
using Zagejmi.Write.Domain.Abstractions;
using Zagejmi.Write.Infrastructure.Ctx;

namespace Zagejmi.Write.Infrastructure.EventStore;

/// <summary>
///     Implements the event store for persisting and retrieving domain events associated with aggregates.
///     Uses event sourcing patterns to maintain the complete history of domain events.
/// </summary>
public class EventStore : IEventStore
{
    /// <summary>
    ///     The JSON serializer options used for deserializing domain events.
    ///     Configured for case-insensitive property matching to handle variations in property naming conventions.
    /// </summary>
    private readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    ///     The database context used for storing and retrieving events.
    /// </summary>
    private readonly ZagejmiWriteContext writeContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EventStore" /> class.
    /// </summary>
    /// <param name="writeContext">The database context for accessing stored events.</param>
    public EventStore(ZagejmiWriteContext writeContext)
    {
        this.writeContext = writeContext;
    }

    /// <summary>
    ///     Asynchronously loads the domain events for a given aggregate ID.
    /// </summary>
    /// <param name="aggregateId">Unique identifier of the aggregate for which to load events.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    ///     A read-only list of domain events associated with the specified aggregate ID, ordered from earliest to latest.
    /// </returns>
    public async Task<IReadOnlyList<IDomainEvent>> LoadAsync(
        Guid aggregateId,
        CancellationToken cancellationToken = default)
    {
        List<Infrastructure.StoredEvent> storedEvents = await this.writeContext.StoredEvents
            .Where(e => e.AggregateId == aggregateId)
            .OrderBy(e => e.Timestamp)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        List<IDomainEvent> domainEvents = [];

        foreach (Infrastructure.StoredEvent storedEvent in storedEvents)
        {
            Type? eventType = Type.GetType(storedEvent.EventType);
            if (eventType == null)
            {
                continue;
            }

            try
            {
                IDomainEvent? domainEvent = (IDomainEvent?)JsonSerializer.Deserialize(
                    storedEvent.Data,
                    eventType,
                    this.jsonSerializerOptions);

                if (domainEvent != null)
                {
                    domainEvents.Add(domainEvent);
                }
            }
            catch
            {
                // Skip events that cannot be deserialized
            }
        }

        return domainEvents.AsReadOnly();
    }

    /// <summary>
    ///     Asynchronously appends a collection of domain events to the event store for a given aggregate ID,
    ///     with optimistic concurrency control based on the expected version.
    /// </summary>
    /// <param name="aggregateId">Unique identifier of the aggregate to which the events belong.</param>
    /// <param name="expectedVersion">
    ///     The expected version of the aggregate's event stream. Used for optimistic concurrency control
    ///     to ensure events are only appended if the current version matches.
    /// </param>
    /// <param name="events">A collection of domain events to be appended to the event store.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation of appending events.</returns>
    public async Task AppendAsync(
        Guid aggregateId,
        long expectedVersion,
        IReadOnlyCollection<IDomainEvent> events,
        CancellationToken cancellationToken = default)
    {
        // Get current event count for optimistic concurrency check
        int currentEventCount = await this.writeContext.StoredEvents
            .CountAsync(e => e.AggregateId == aggregateId, cancellationToken);

        if (currentEventCount != expectedVersion)
        {
            throw new InvalidOperationException(
                $"Concurrency conflict: Expected version {expectedVersion} but found {currentEventCount} events for aggregate {aggregateId}");
        }

        // Append new events to the store
        foreach (IDomainEvent @event in events)
        {
            Type eventType = @event.GetType();
            Infrastructure.StoredEvent storedEvent = new()
            {
                Id = Guid.CreateVersion7(),
                Timestamp = new DateTime(DateTime.UtcNow.Ticks, DateTimeKind.Unspecified),
                AggregateId = aggregateId,
                EventType = $"{eventType.FullName}, {eventType.Assembly.GetName().Name}",
                Data = JsonSerializer.Serialize(@event, eventType)
            };

            this.writeContext.StoredEvents.Add(storedEvent);
        }

        await this.writeContext.SaveChangesAsync(cancellationToken);
    }
}