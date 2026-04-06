using System.Collections.Generic;
using LanguageExt;
using Zagejmi.Shared.Failures;

namespace Zagejmi.Write.Domain.Auth.Verification;

public sealed record VerificationPersonal(Profile.Profile? Verifier, string Note) : Verification
{
    public readonly string? Note = Note;
    public readonly Profile.Profile? Verifier = Verifier;

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Type;
        yield return Verifier;
        yield return Note;
    }

    public override Either<Failure, bool> Verify(params object?[] args)
    {
        return Verifier != null;
    }
}