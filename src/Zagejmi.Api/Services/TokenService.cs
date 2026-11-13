using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using Zagejmi.Application.Auth.Tokens;
using Zagejmi.Domain.Auth;

namespace Zagejmi.Api.Services;

/// <summary>
///     Service for generating JWT authentication tokens for user profiles.
/// </summary>
public class TokenService : ITokenService
{
    /// <summary>
    ///     Configuration for accessing JWT settings such as secret key, issuer, and audience.
    /// </summary>
    private readonly IConfiguration configuration;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TokenService" /> class with the specified configuration.
    /// </summary>
    /// <param name="configuration">
    ///     The configuration object used to access JWT settings such as secret key, issuer, and
    ///     audience.
    /// </param>
    public TokenService(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    /// <inheritdoc />
    public string GenerateRefreshToken()
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    /// <inheritdoc />
    public string HashRefreshToken(string refreshToken)
    {
        byte[] input = Encoding.UTF8.GetBytes(refreshToken);
        byte[] hash = SHA256.HashData(input);
        return Convert.ToHexString(hash);
    }

    /// <inheritdoc />
    public bool TryGetUserId(string jwtToken, out Guid userId)
    {
        userId = Guid.Empty;

        if (string.IsNullOrWhiteSpace(jwtToken))
        {
            return false;
        }

        try
        {
            JwtSecurityToken token = new JwtSecurityTokenHandler().ReadJwtToken(jwtToken);
            string? subject = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

            return Guid.TryParse(subject, out userId);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Generates a JWT token for the given user.
    /// </summary>
    /// <param name="user"> The user for which to generate the token.</param>
    /// <returns>Generated token for the user.</returns>
    public string GenerateToken(User user)
    {
        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(this.configuration["Jwt:Key"]!));
        SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256);

        Claim[] claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.AuthCredentials?.Email ?? "unknown"),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.AuthCredentials?.UserName ?? "unknown"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
        };

        JwtSecurityToken token = new(
            this.configuration["Jwt:Issuer"],
            this.configuration["Jwt:Audience"],
            claims,
            expires: DateTime.UtcNow.AddMinutes(this.GetAccessTokenLifetimeMinutes()),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc />
    public TimeSpan RefreshTokenLifetime
    {
        get
        {
            string? configured = this.configuration["Jwt:RefreshTokenDays"];
            return int.TryParse(configured, NumberStyles.Integer, CultureInfo.InvariantCulture, out int days) && days > 0
                ? TimeSpan.FromDays(days)
                : TimeSpan.FromDays(30);
        }
    }

    private int GetAccessTokenLifetimeMinutes()
    {
        string? configured = this.configuration["Jwt:AccessTokenMinutes"];
        if (int.TryParse(configured, NumberStyles.Integer, CultureInfo.InvariantCulture, out int minutes) &&
            minutes > 0)
        {
            return minutes;
        }

        return 120;
    }
}
