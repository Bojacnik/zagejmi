using System.ComponentModel;
using Zagejmi.Domain.Auth.Hashing;
using Zagejmi.Domain.Entity;

namespace Zagejmi.Domain.Auth;

public sealed record Password : ValueObject
{
    #region Password properties

    [PasswordPropertyText] public readonly string Value;
    [PasswordPropertyText] public readonly string Salt;
    public readonly HashType HashType;

    #endregion

    public Password(string hashedPassword, string salt, HashType hashType)
    {
        Value = hashedPassword;
        Salt = salt;
        HashType = hashType;
    }

    /// <summary>
    /// Factory method to create a new Password instance by hashing a plaintext password.
    /// This is the only safe way to create a new password.
    /// </summary>
    public static Password Create(
        string plainTextPassword,
        string salt,
        IHashHandler hashHandler,
        HashType hashType)
    {
        string hashedPassword = hashHandler.Hash(plainTextPassword, salt, hashType);
        return new Password(hashedPassword, salt, hashType);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
        yield return Salt;
        yield return HashType;
    }
}