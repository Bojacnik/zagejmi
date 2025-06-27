using SharedKernel;

namespace Zagejmi.Domain.Community.Goin;

public sealed class Goin(ulong amount) : ValueObject
{
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return amount;
    }
}