namespace SharedKernel;

public abstract class AggregateRoot<TDomainEvent> where TDomainEvent : IDomainEvent
{
    public abstract ulong Id { get; }
    protected abstract ulong Version { get; set; }
    private readonly List<TDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<TDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(TDomainEvent @event)
    {
        _domainEvents.Add(@event);
    }

    public void LoadFromHistory(IEnumerable<TDomainEvent> history)
    {
        foreach (TDomainEvent @event in history)
        {
            ApplyEvent(@event, true);
        }
    }

    private void ApplyEvent(TDomainEvent @event, bool fromHistory = false)
    {
        // Apply event to aggregate

        // TODO: FIX ME (tohle by ani Bořek Stavitel nespravil)
        //((dynamic)this).Handle((dynamic)@event);

        if (fromHistory) return;

        checked
        {
            Version += @event.Version;
        }

        AddDomainEvent(@event);
    }
}