using LanguageExt;

using Zagejmi.Contracts.Failures;

namespace Zagejmi.Write.Domain.Auth.Verification;

public sealed record VerificationFace(string? Path) : Verification
{
    public readonly string? Path = Path;

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return this.Type;
        yield return this.Path;
    }

    public override Either<Failure, bool> Verify(params object?[] args)
    {
        return this.Path != null;
    }
}