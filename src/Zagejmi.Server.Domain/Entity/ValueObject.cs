namespace Zagejmi.Server.Domain.Entity;

public abstract record ValueObject
{
    protected abstract IEnumerable<object?> GetAtomicValues();

    public override int GetHashCode()
    {
        return GetAtomicValues()
            .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);
    }
}