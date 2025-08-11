using LanguageExt;
using Zagejmi.Server.Write.Domain.Entity;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Server.Write.Domain.Community.People.Verification;

public abstract record Verification : ValueObject
{
    public VerificationType Type { get; init; }

    public abstract Either<Failure, bool> Verify(params object?[] args);
}