using LanguageExt;
using SharedKernel;
using SharedKernel.Failures;

namespace Zagejmi.Domain.Community.User.Verification;

public abstract class Verification : ValueObject
{
    public VerificationType Type { get; init; }

    public abstract Either<Failure, bool> Verify(params object?[] args);
}