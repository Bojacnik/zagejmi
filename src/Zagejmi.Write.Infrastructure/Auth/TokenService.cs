using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using Zagejmi.Write.Application;
using Zagejmi.Write.Domain.Profile;

namespace Zagejmi.Write.Infrastructure.Auth;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        this._configuration = configuration;
    }

    public string GenerateToken(Profile profile)
    {
        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(this._configuration["Jwt:Key"]!));
        SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256);

        if (profile.PersonalInformation.MailAddress == null)
        {
            throw new NotImplementedException();
        }

        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, profile.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, profile.PersonalInformation.MailAddress ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];

        JwtSecurityToken token = new(
            this._configuration["Jwt:Issuer"],
            this._configuration["Jwt:Audience"],
            claims,
            expires: DateTime.Now.AddMinutes(120),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}