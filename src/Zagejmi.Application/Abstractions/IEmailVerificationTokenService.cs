namespace Zagejmi.Application.Abstractions;

public interface IEmailVerificationTokenService
{
    string GenerateToken();

    string HashToken(string token);
}