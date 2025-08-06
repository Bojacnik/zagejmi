using LanguageExt;
using Zagejmi.Domain.Entity;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Domain.Community.People.Verification;

public abstract record Verification : ValueObject
{
    public VerificationType Type { get; init; }

    public abstract Either<Failure, bool> Verify(params object?[] args);
}