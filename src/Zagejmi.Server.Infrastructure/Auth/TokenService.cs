using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Zagejmi.Server.Application;
using Zagejmi.Server.Domain.Community.People;

namespace Zagejmi.Server.Infrastructure.Auth;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(Person person)
    {
        SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        if (person.PersonalInformation.MailAddress == null) throw new NotImplementedException();

        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, person.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, person.PersonalInformation.MailAddress ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            // You can add more claims here, like roles
            // new Claim(ClaimTypes.Role, person.PersonType.ToString())
        ];

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(120),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}