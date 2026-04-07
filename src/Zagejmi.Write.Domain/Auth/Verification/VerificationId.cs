using System.Collections.Generic;

using LanguageExt;

using SixLabors.ImageSharp;

using Zagejmi.Shared.Failures;

namespace Zagejmi.Write.Domain.Auth.Verification;

public sealed record VerificationId(Image imageFront, Image imageBack) : Verification
{
    public readonly Image ImageBack = imageBack;
    public readonly Image ImageFront = imageFront;

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return this.Type;
        yield return this.ImageFront;
        yield return this.ImageBack;
    }

    public override Either<Failure, bool> Verify(params object?[] args)
    {
        return this.ImageFront != null && this.ImageBack != null;
    }
}