using Zagejmi.Server.Domain.Community.People;

namespace Zagejmi.Server.Application;

public interface ITokenService
{
    string GenerateToken(Person person);
}