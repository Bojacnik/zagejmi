using Zagejmi.Server.Write.Domain.Community.People;

namespace Zagejmi.Server.Write.Application;

public interface ITokenService
{
    string GenerateToken(Person person);
}