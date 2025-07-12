namespace SharedKernel;

public abstract record Entity<TId> where TId : notnull
{
    public TId Id { get; protected init; }

    protected Entity(TId id)
    {
        // An entity must have an ID from the start.
        Id = id;
    }
    
    public override int GetHashCode() => Id.GetHashCode();
}