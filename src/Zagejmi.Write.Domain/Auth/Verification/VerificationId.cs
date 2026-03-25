using System.Drawing;

using LanguageExt;

using Zagejmi.Contracts.Failures;

namespace Zagejmi.Write.Domain.Auth.Verification;

public sealed record VerificationId(Image? ImageFront, Image? ImageBack) : Verification
{
    public readonly Image? ImageBack = ImageBack;
    public readonly Image? ImageFront = ImageFront;

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