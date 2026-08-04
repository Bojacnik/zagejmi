using System.Collections.Generic;

namespace Zagejmi.Domain.Auth.Verification;

public sealed record VerificationPersonal(Profile.Abstractions.Profile? Verifier, string Note)
    : Abstractions.Verification
{
    public readonly string? Note = Note;
    public readonly Profile.Abstractions.Profile? Verifier = Verifier;

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return this.Type;
        yield return this.Verifier;
        yield return this.Note;
    }

    public override (bool? Result, Failure? Error) Verify(params object?[] args)
{
    return (this.Verifier != null, null);
}
}