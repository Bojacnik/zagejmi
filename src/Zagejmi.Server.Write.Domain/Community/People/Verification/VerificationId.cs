using System.Drawing;
using LanguageExt;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Server.Write.Domain.Community.People.Verification;

public sealed record VerificationId(Image? ImageFront, Image? ImageBack) : People.Verification.Verification
{
    public readonly Image? ImageFront = ImageFront;
    public readonly Image? ImageBack = ImageBack;

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