using System.Drawing;

namespace Zagejmi.Domain.Community.User.Verification;

public class VerificationId(Image imageFront, Image imageBack) : Verification
{
    public Image ImageFront = imageFront;
    public Image ImageBack = imageBack;
}