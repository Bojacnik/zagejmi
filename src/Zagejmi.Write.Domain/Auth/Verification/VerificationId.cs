using System.Collections.Generic;
using System.Drawing;
using LanguageExt;
using Zagejmi.Shared.Failures;

namespace Zagejmi.Write.Domain.Auth.Verification;

public sealed record VerificationId(Image? ImageFront, Image? ImageBack) : Verification
{
    public readonly Image? ImageBack = ImageBack;
    public readonly Image? ImageFront = ImageFront;

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Type;
        yield return ImageFront;
        yield return ImageBack;
    }

    public override Either<Failure, bool> Verify(params object?[] args)
    {
        return ImageFront != null && ImageBack != null;
    }
}