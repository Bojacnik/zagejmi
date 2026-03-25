using System;

namespace Zagejmi.Write.Domain.Abstractions;

/// <summary>
///     Represents a domain event in the system.
///     All domain events should implement this interface.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    ///     Gets the unique identifier of the aggregate that raised the event.
    /// </summary>
    Guid AggregateId { get; }

    /// <summary>
    ///     Gets the UTC timestamp of when the event occurred.
    /// </summary>
    DateTimeOffset OccurredOn { get; }

    /// <summary>
    ///     Gets optional version of the aggregate at the time of the event.
    ///     Useful for optimistic concurrency.
    /// </summary>
    int? AggregateVersion { get; }
}