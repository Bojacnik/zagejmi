using Zagejmi.Domain.Abstractions;

namespace Zagejmi.Domain.Auth.Verification.Abstractions;

public abstract record Verification : ValueObject
{
    public VerificationType Type { get; init; }

    public abstract Result<bool> Verify(params object?[] args);
}