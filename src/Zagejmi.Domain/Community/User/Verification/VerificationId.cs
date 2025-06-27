using System.Drawing;

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
}