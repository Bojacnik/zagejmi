using Zagejmi.Server.Domain.Auth.Hashing.Hashers;

namespace Zagejmi.Server.Domain.Auth.Hashing;

public class HashHandler : IHashHandler
{
    private readonly IHasher[] _hashers;

    public HashHandler(IHasher[] hashers)
    {
        _hashers = hashers;
    }

    private IHasher _findHasherByType(HashType hashType)
    {
        return _hashers
            .First(x => hashType switch
            {
                HashType.Sha256 => x is HasherSha256,
                _ => false
            });
    }

    public string Hash(string password, string salt, HashType hashType)
    {
        IHasher hasher = _findHasherByType(hashType);
        return hasher.Hash(password, salt);
    }
}