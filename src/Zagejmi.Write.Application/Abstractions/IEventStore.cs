using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zagejmi.Write.Domain.Events;

namespace Zagejmi.Write.Application.Abstractions;

/// <summary>
/// Defines the contract for an event store, which is responsible for persisting and retrieving domain events associated with aggregates.
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Asynchronously loads the domain events for a given aggregate ID.
    /// </summary>
    /// <param name="aggregateId">Unique identifier of the aggregate for which to load events.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>Returns a sorted list from the earliest to the latest of domain events associated with the specified aggregate ID.</returns>
    Task<IReadOnlyList<IDomainEvent>> LoadAsync(
        Guid aggregateId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously appends a collection of domain events to the event store for a given aggregate ID, with optimistic concurrency control based on the expected version.
    /// </summary>
    /// <param name="aggregateId">Unique identifier of the aggregate to which the events belong.</param>
    /// <param name="expectedVersion">The expected version of the aggregate's event stream. This is used for optimistic concurrency control to ensure that events are only appended if the current version matches the expected version.</param>
    /// <param name="events">A collection of domain events to be appended to the event store for the specified aggregate ID.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation of appending events to the event store. The task completes when the events have been successfully appended or if an error occurs.</returns>
    Task AppendAsync(
        Guid aggregateId,
        long expectedVersion,
        IReadOnlyCollection<IDomainEvent> events,
        CancellationToken cancellationToken = default);
}