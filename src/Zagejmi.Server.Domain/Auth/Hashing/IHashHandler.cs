namespace Zagejmi.Server.Domain.Auth.Hashing;

public interface IHashHandler
{
    string Hash(string password, string salt, HashType hashType);
}