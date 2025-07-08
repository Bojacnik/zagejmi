using SharedKernel;

namespace Zagejmi.Domain.Community.Goin;

public sealed class Goin : ValueObject
{
    public readonly ulong Amount;

    public Goin(ulong amount)
    {
        Amount = amount;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Amount;
    }
}