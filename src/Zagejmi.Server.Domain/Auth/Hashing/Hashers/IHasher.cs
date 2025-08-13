namespace Zagejmi.Server.Domain.Auth.Hashing.Hashers;

public interface IHasher
{
    string Hash(string password, string salt);
}