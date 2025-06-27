namespace Zagejmi.Domain.Community.User.Verification;

public sealed class VerificationFace(string? path) : Verification
{
    public string? Path = path;
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Type;
        yield return Path;
    }
}