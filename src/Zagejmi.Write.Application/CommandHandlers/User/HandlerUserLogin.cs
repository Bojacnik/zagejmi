using LanguageExt;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Zagejmi.Write.Domain.Auth;
using Zagejmi.Write.Domain.Repository;
using Zagejmi.SharedKernel.Failures;
using Zagejmi.Write.Application.Commands.User;

namespace Zagejmi.Write.Application.CommandHandlers.User;

public class HandlerUserLogin : IHandlerRequest<CommandUserLogin, Either<Failure, string>>
{
    private readonly IRepositoryUserWrite _repositoryUserWrite;
    private readonly IConfiguration _configuration;
    private readonly IHashHandler _hashHandler;

    public HandlerUserLogin(
        IRepositoryUserWrite repositoryUserWrite,
        IConfiguration configuration,
        IHashHandler hashHandler)
    {
        _repositoryUserWrite = repositoryUserWrite;
        _configuration = configuration;
        _hashHandler = hashHandler;
    }

    public async Task<Either<Failure, string>> Handle(CommandUserLogin request, CancellationToken cancellationToken)
    {
        try
        {
            Option<Domain.Auth.User> userOption = await _repositoryUserWrite.GetByUsernameAsync(request.Username);

            return userOption.Match<Either<Failure, string>>(
                Some: user =>
                {
                    if (!user.IsPasswordValid(request.Password, _hashHandler))
                    {
                        return new VerificationFailureInvalidLogin("Invalid username or password.");
                    }

                    var token = GenerateJwtToken(user);
                    return token;
                },
                None: () => new VerificationFailureInvalidLogin("Invalid username or password.")
            );
        }
        catch (Exception ex)
        {
            return new FailureArgumentInvalidValue($"An unexpected error occurred during login: {ex.Message}");
        }
    }

    private string GenerateJwtToken(Domain.Auth.User user)
    {
        SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)
        };

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(120),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}