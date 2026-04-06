using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using Zagejmi.Write.Application.Abstractions;
using Zagejmi.Write.Domain.Auth;

namespace Zagejmi.Write.Infrastructure.Auth;

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

    /// <summary>
    ///     Generates a JWT token for the given user.
    /// </summary>
    /// <param name="user"> The user for which to generate the token.</param>
    /// <returns>Generated token for the user.</returns>
    public string GenerateToken(User user)
    {
        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(this.configuration["Jwt:Key"]!));
        SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256);

        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.AuthCredentials.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];

        JwtSecurityToken token = new(
            this.configuration["Jwt:Issuer"],
            this.configuration["Jwt:Audience"],
            claims,
            expires: DateTime.Now.AddMinutes(120),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}