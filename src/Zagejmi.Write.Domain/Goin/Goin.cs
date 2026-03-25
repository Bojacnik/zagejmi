using Zagejmi.Write.Domain.Abstractions;

namespace Zagejmi.Write.Domain.Goin;

public sealed record Goin(ulong Amount) : ValueObject
{
    public readonly ulong Amount = Amount;

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return this.Amount;
    }
}