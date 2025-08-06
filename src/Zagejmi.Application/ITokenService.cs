using Zagejmi.Domain.Community.People;

namespace Zagejmi.Application;

public interface ITokenService
{
    string GenerateToken(Person person);
}