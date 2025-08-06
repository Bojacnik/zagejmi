using Zagejmi.Domain.Community.People;
using Zagejmi.Domain.Community.People.Person;

namespace Zagejmi.Application;

public interface ITokenService
{
    string GenerateToken(Person person);
}