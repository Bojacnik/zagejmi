using System.Drawing;
using LanguageExt;
using SharedKernel;
using SharedKernel.Failures;

namespace Zagejmi.Domain.Community.User.Verification;

public sealed class VerificationId(Image? imageFront, Image? imageBack) : Verification
{
    public Image? ImageFront = imageFront;
    public Image? ImageBack = imageBack;

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