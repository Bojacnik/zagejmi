using System;
using System.Security.Cryptography;
using System.Text;

using Zagejmi.Application.Abstractions;

namespace Zagejmi.Api.Services;

public sealed class EmailVerificationTokenService : IEmailVerificationTokenService
{
    public string GenerateToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    public string HashToken(string token)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(token);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}