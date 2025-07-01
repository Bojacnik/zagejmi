namespace SharedKernel;

public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; init; }

    protected Entity(TId id)
    {
        // An entity must have an ID from the start.
        Id = id;
    }

    // Boilerplate for identity comparison
    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;
        
        // If the IDs are the default value (e.g., 0 for int, Guid.Empty for Guid),
        // they haven't been initialized, so they can't be equal.
        if (Id.Equals(default) || other.Id.Equals(default))
            return false;

        return Id.Equals(other.Id);
    }

    public static bool operator ==(Entity<TId>? a, Entity<TId>? b)
    {
        if (a is null && b is null)
            return true;
        if (a is null || b is null)
            return false;
        return a.Equals(b);
    }

    public static bool operator !=(Entity<TId>? a, Entity<TId>? b) => !(a == b);

    public override int GetHashCode() => Id.GetHashCode();
}