using Zagejmi.Domain.Abstractions;

namespace Zagejmi.Domain.Auth.Verification.Abstractions;

public abstract record Verification : ValueObject
{
    public VerificationType Type { get; init; }

    public abstract (bool? Result, Failure? Error) Verify(params object?[] args);
}