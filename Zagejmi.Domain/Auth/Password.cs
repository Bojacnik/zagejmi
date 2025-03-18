using System.ComponentModel;
using Zagejmi.Domain.Auth.Hashing;

namespace Zagejmi.Domain.Auth;

public class Password
{
    [PasswordPropertyText]
    public string Salt;
    
    [PasswordPropertyText]
    public string HashedPassword;

    private readonly HashType _hashType;
    private readonly IHashHandler _hashHandler;

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
}