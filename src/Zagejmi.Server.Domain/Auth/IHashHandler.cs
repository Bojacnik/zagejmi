namespace Zagejmi.Server.Domain.Auth;

// Defines the contract for a service that can hash and verify passwords.
public interface IHashHandler
{
    (string Hash, string Salt) Hash(string password);
    bool Verify(string password, string hash, string salt);
}