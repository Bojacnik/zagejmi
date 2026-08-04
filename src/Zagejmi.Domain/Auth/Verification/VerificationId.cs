using System.Collections.Generic;

using SixLabors.ImageSharp;

namespace Zagejmi.Domain.Auth.Verification;

public sealed record VerificationId(Image ImageFront, Image ImageBack) : Abstractions.Verification
{
    public readonly Image ImageBack = ImageBack;
    public readonly Image ImageFront = ImageFront;

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return (this.Type, null);
        yield return (this.ImageFront, null);
        yield return (this.ImageBack, null);
    }

    public override (bool? Result, Failure? Error) Verify(params object?[] args)
{
    return (this.ImageFront != null && this.ImageBack != null, null);
}
}