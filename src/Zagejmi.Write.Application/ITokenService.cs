using Zagejmi.Write.Domain.Profile;

namespace Zagejmi.Write.Application;

public interface ITokenService
{
    string GenerateToken(Profile profile);
}