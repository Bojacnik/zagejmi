using System.ComponentModel;
using SharedKernel;
using Zagejmi.Domain.Auth.Hashing;

namespace Zagejmi.Domain.Auth;

public sealed record Password : ValueObject
{
    #region Password properties

    [PasswordPropertyText] public string Salt;

    [PasswordPropertyText] public string HashedPassword;

    private readonly HashType _hashType;
    private readonly IHashHandler _hashHandler;

    #endregion

    private string Hash(string password, string salt, HashType hashType)
    {
        return _hashHandler.Hash(
            password,
            salt,
            hashType
        );
    }

    public Password(string password, string salt, HashType hashType, IHashHandler hashHandler)
    {
        Salt = salt;
        _hashType = hashType;
        _hashHandler = hashHandler;
        HashedPassword = Hash(password, salt, hashType);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Salt;
        yield return HashedPassword;
        yield return _hashType;
        yield return _hashHandler;
    }
}