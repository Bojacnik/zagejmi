using Zagejmi.Domain.Entity;

namespace Zagejmi.Domain.Community.Goin;

public sealed record Goin(ulong Amount) : ValueObject
{
    public readonly ulong Amount = Amount;

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Amount;
    }
}