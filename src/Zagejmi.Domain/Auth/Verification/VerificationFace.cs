using System.Collections.Generic;

namespace Zagejmi.Domain.Auth.Verification;

public sealed record VerificationFace(string? Path) : Abstractions.Verification
{
    public readonly string? Path = Path;

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return this.Type;
        yield return this.Path;
    }

    public override (object? Result, Failure? Error) Verify(params object?[] args)
    {
        return this.Path != null;
    }
}