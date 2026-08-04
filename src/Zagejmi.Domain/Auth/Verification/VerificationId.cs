using System.Collections.Generic;

using SixLabors.ImageSharp;

namespace Zagejmi.Domain.Auth.Verification;

public sealed record VerificationId(Image ImageFront, Image ImageBack) : Abstractions.Verification
{
    public readonly Image ImageBack = ImageBack;
    public readonly Image ImageFront = ImageFront;

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return this.Type;
        yield return this.ImageFront;
        yield return this.ImageBack;
    }

    public override (bool? Result, Failure? Error) Verify(params object?[] args)
{
    return (this.ImageFront != null && this.ImageBack != null, null);
}
}