using LanguageExt;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Domain.Community.People.Verification;

public sealed record VerificationFace(string? Path) : People.Verification.Verification
{
    public readonly string? Path = Path;

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Type;
        yield return Path;
    }

    public override Either<Failure, bool> Verify(params object?[] args)
    {
        return Path != null;
    }
}