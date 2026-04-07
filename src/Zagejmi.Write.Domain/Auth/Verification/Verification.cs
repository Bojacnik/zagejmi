using LanguageExt;

using Zagejmi.Shared.Failures;
using Zagejmi.Write.Domain.Abstractions;

namespace Zagejmi.Write.Domain.Auth.Verification;

public abstract record Verification : ValueObject
{
    public VerificationType Type { get; init; }

    public abstract Either<Failure, bool> Verify(params object?[] args);
}