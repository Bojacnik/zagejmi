namespace Zagejmi.Server.Write.Domain.Entity;

public abstract class AggregateRoot<TId>(TId id, uint version = 0) : Entity<TId>(id) where TId : notnull
{
    protected uint Version { get; set; } = version;
}

public abstract class AggregateRoot<TAggregateRoot, TId>(TId id) : AggregateRoot<TId>(id)
    where TAggregateRoot : AggregateRoot<TAggregateRoot, TId>
    where TId : notnull
{
    private readonly List<IDomainEvent<TAggregateRoot, TId>> _domainEvents = [];

    public IReadOnlyList<IDomainEvent<TAggregateRoot, TId>> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent<TAggregateRoot, TId> newEvent)
    {
        Apply(newEvent);

        _domainEvents.Add(newEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public void LoadFromHistory(IEnumerable<IDomainEvent<TAggregateRoot, TId>> history)
    {
        foreach (IDomainEvent<TAggregateRoot, TId> evt in history)
        {
            Apply(evt);
            Version++;
        }
    }

    protected abstract void Apply(IDomainEvent<TAggregateRoot, TId> evt);
}