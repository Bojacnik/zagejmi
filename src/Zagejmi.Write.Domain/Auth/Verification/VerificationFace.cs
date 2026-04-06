using System.Collections.Generic;
using LanguageExt;
using Zagejmi.Shared.Failures;

namespace Zagejmi.Write.Domain.Auth.Verification;

public sealed record VerificationFace(string? Path) : Verification
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