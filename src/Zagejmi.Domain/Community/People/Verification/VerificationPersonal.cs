using LanguageExt;
using SharedKernel.Failures;

namespace Zagejmi.Domain.Community.People.Verification;

public sealed record VerificationPersonal(Person? Verifier, string Note) : Verification
{
    public readonly Person? Verifier = Verifier;
    public readonly string? Note = Note;

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