using System.ComponentModel;
using SharedKernel;
using Zagejmi.Domain.Auth.Hashing;

namespace Zagejmi.Domain.Auth;

public sealed record Password : ValueObject
{
    #region Password properties

    [PasswordPropertyText] public readonly string Value;
    [PasswordPropertyText] public readonly string Salt;
    public readonly HashType HashType;

    #endregion

    private static string Hash(string password, string salt, HashType hashType, IHashHandler hashHandler)
    {
        return hashHandler.Hash(
            password,
            salt,
            hashType
        );
    }

    public Password(string password, string salt, HashType hashType)
    {
        Value = password;
        Salt = salt;
        HashType = hashType;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
        yield return Salt;
        yield return HashType;
    }
}