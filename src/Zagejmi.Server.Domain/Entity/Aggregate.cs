using Zagejmi.Server.Domain.Events;

namespace Zagejmi.Server.Domain.Entity;

public abstract class Aggregate<TId>(TId id, uint version = 0) : Entity<TId>(id) where TId : notnull
{
    public uint Version { get; protected set; } = version;
}

public abstract class Aggregate<TAggregate, TId>(TId id) : Aggregate<TId>(id)
    where TAggregate : Aggregate<TAggregate, TId>
    where TId : notnull
{
    private readonly List<IDomainEvent<TAggregate, TId>> _domainEvents = [];
    public IReadOnlyList<IDomainEvent<TAggregate, TId>> DomainEvents => _domainEvents.AsReadOnly();

    public void LoadFromHistory(IEnumerable<IDomainEvent<TAggregate, TId>> history)
    {
        foreach (var evt in history)
        {
            Apply(evt);
            Version++;
        }
    }

    protected void RaiseEvent(IDomainEvent<TAggregate, TId> evt)
    {
        Apply(evt);
        _domainEvents.Add(evt);
    }

    public void ClearEvents()
    {
        _domainEvents.Clear();
    }

    protected abstract void Apply(IDomainEvent<TAggregate, TId> evt);
}