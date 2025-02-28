using Zagejmi.Domain.Community.User;
using Zagejmi.Domain.Events;

namespace Zagejmi.Application.Repository;

public interface IPersonRepository : IDefaultRepository<Person, IPersonEvent>
{
    Task<Person> GetByEmailAsync(string email);
    Task<Person> GetByUsernameAsync(string username);
}