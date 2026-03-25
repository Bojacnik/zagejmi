using LanguageExt;

using Zagejmi.Contracts.Failures;

namespace Zagejmi.Write.Domain.Auth.Verification;

public sealed record VerificationPersonal(Profile.Profile? Verifier, string Note) : Verification
{
    public readonly string? Note = Note;
    public readonly Profile.Profile? Verifier = Verifier;

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return this.Type;
        yield return this.Verifier;
        yield return this.Note;
    }

    public override Either<Failure, bool> Verify(params object?[] args)
    {
        return this.Verifier != null;
    }
}