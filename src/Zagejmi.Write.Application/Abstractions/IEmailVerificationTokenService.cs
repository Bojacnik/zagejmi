namespace Zagejmi.Write.Application.Abstractions;

public interface IEmailVerificationTokenService
{
    string GenerateToken();

    string HashToken(string token);
}