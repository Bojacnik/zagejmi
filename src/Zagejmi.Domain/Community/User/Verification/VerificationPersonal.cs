using LanguageExt;
using SharedKernel;
using SharedKernel.Failures;

namespace Zagejmi.Domain.Community.User.Verification;

public sealed class PersonalVerification(Person? verifier, string note) : Verification
{
    public Person? Verifier = verifier;
    public string? Note = note;

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