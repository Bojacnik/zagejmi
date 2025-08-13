using Zagejmi.Server.Domain.Entity;

namespace Zagejmi.Server.Domain.Auth;

// Represents a securely hashed password, storing the hash and the salt.
public record Password : ValueObject
{
    public string Hash { get; }
    public string Salt { get; }

    public Password(string hash, string salt)
    {
        Hash = hash;
        Salt = salt;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Hash;
        yield return Salt;
    }
}