using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Zagejmi.Server.Domain.Auth;

namespace Zagejmi.Server.Infrastructure.Auth;

public class HashHandler : IHashHandler
{
    private const int SALT_SIZE = 16; // 128 bits

    public (string Hash, string Salt) Hash(string password)
    {
        // 1. Generate a cryptographic random salt using a secure generator.
        byte[] salt = RandomNumberGenerator.GetBytes(SALT_SIZE);

        // 2. Hash the password with the salt using Argon2id.
        Argon2id argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = 8, // Use 8 threads
            Iterations = 4,          // 4 passes
            MemorySize = 1024 * 128  // 128 MB memory
        };

        byte[] hash = argon2.GetBytes(32); // 256-bit hash

        // 3. Return the hash and salt as Base64 strings for easy storage.
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public bool Verify(string password, string hash, string salt)
    {
        byte[] saltBytes = Convert.FromBase64String(salt);
        byte[] hashBytes = Convert.FromBase64String(hash);

        // Re-hash the provided password with the original salt and parameters.
        Argon2id argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)) { Salt = saltBytes, DegreeOfParallelism = 8, Iterations = 4, MemorySize = 1024 * 128 };
        byte[] newHash = argon2.GetBytes(32);

        // Use a constant-time comparison to prevent timing attacks.
        return CryptographicOperations.FixedTimeEquals(hashBytes, newHash);
    }
}