using Zagejmi.Server.Write.Domain.Entity;

namespace Zagejmi.Server.Write.Domain.Community.Goin;

public sealed record Goin(ulong Amount) : ValueObject
{
    public readonly ulong Amount = Amount;

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Amount;
    }
}