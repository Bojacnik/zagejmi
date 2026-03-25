using System;
using System.Collections.Generic;

namespace Zagejmi.Write.Domain.Abstractions;

/// <summary>
///     Base class for all aggregates in the domain.
///     Inherits from Entity and manages domain events and versioning.
/// </summary>
public abstract class Aggregate : Entity
{
    /// <summary>
    ///     List of domain events that have been raised but not yet persisted. This is used to track changes to the aggregate
    ///     state and to publish events after successful persistence.
    /// </summary>
    private readonly List<IDomainEvent> domainEvents = [];

    /// <summary>
    ///     Initializes a new instance of the <see cref="Aggregate" /> class with the specified unique identifier and version.
    /// </summary>
    /// <param name="id">The unique identifier for the aggregate. Must not be empty.</param>
    /// <param name="version">The initial version of the aggregate. Defaults to 0 for new aggregates.</param>
    protected Aggregate(Guid id, int version = 0)
        : base(id)
    {
        this.Version = version;
    }

    /// <summary>
    ///     Gets or sets aggregate version for optimistic concurrency control.
    /// </summary>
    public int Version { get; protected set; }

    /// <summary>
    ///     Gets a read-only list of domain events that have been raised but not yet persisted. This allows external code (like
    ///     repositories or event dispatchers) to access the events without modifying the internal list.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => this.domainEvents.AsReadOnly();

    /// <summary>
    ///     Loads aggregate state from a history of domain events.
    /// </summary>
    public void LoadFromHistory(IEnumerable<IDomainEvent> history)
    {
        foreach (IDomainEvent @event in history)
        {
            this.Apply(@event);
            this.Version++;
        }
    }

    /// <summary>
    ///     Clears all accumulated domain events.
    /// </summary>
    public void ClearEvents()
    {
        this.domainEvents.Clear();
    }

    /// <summary>
    ///     Raises a new domain event and applies it to the aggregate state.
    ///     This method should be called whenever a state change occurs in the aggregate that should be captured as a domain
    ///     event.
    ///     The event is applied to the aggregate state, the version is incremented, and the event is added to the list of
    ///     domain events to be persisted and published later.
    /// </summary>
    protected void RaiseEvent(IDomainEvent evt)
    {
        this.Apply(evt);
        this.domainEvents.Add(evt);
    }

    /// <summary>
    ///     Apply a domain event to the aggregate state. Must be implemented in concrete aggregates.
    /// </summary>
    protected abstract void Apply(IDomainEvent @event);
}