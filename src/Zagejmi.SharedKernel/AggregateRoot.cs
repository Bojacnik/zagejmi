namespace SharedKernel;

internal abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    // The current version of the aggregate. Used for optimistic concurrency.
    public int Version { get; protected set; }

    // Provides access to the uncommitted events.
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot(TId id) : base(id)
    {
    }

    /// <summary>
    /// Adds an event to the list of uncommitted changes and immediately applies it
    /// to the aggregate to update its state.
    /// </summary>
    protected void AddDomainEvent(IDomainEvent newEvent)
    {
        // 1. Ensure the event is applied to the aggregate to update its state.
        Apply(newEvent);

        // 2. Add the event to the list of uncommitted changes.
        _domainEvents.Add(newEvent);
    }

    /// <summary>
    /// Clears the list of uncommitted domain events.
    /// This is called by the repository after the events have been persisted.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Rehydrates the aggregate's state by applying a series of historical events.
    /// </summary>
    public void LoadFromHistory(IEnumerable<IDomainEvent> history)
    {
        foreach (var evt in history)
        {
            Apply(evt);
            Version++; // Increment version for each historical event
        }
    }

    /// <summary>
    /// The magic method that routes an event to the correct state-mutating method.
    /// It uses dynamic dispatch to call the appropriate `Apply(SpecificEventType evt)` overload.
    /// </summary>
    protected abstract void Apply(IDomainEvent evt);
}