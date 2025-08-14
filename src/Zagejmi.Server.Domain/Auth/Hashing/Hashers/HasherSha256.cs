using System.Security.Cryptography;
using System.Text;

namespace Zagejmi.Server.Domain.Auth.Hashing.Hashers;

public class HasherSha256 : IHasher
{
    private static byte[] Combine(byte[] first, byte[] second)
    {
        byte[] result = new byte[first.Length + second.Length];
        Buffer.BlockCopy(first, 0, result, 0, first.Length);
        Buffer.BlockCopy(second, 0, result, first.Length, second.Length);
        return result;
    }

    public string Hash(string password, string salt)
    {
        // Combine salt and input
        byte[] combined = Combine(Encoding.UTF8.GetBytes(salt), Encoding.UTF8.GetBytes(password));

        // Create a SHA256 hash object
        // Compute the hash
        byte[] hashBytes = SHA256.HashData(combined);

        // Convert the hash bytes to a hexadecimal string
        return Convert.ToHexStringLower(hashBytes);
    }
}