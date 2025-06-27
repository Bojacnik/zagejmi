using Zagejmi.Domain.Events;
using Zagejmi.Domain.Community.User;

namespace Zagejmi.Domain.Repository;

public interface IPersonRepository : IDefaultRepository<Person, IPersonEvent>
{
    Task<Person> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<Person> GetByUsernameAsync(string username, CancellationToken cancellationToken);
}