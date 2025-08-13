using Zagejmi.Server.Domain.Community.People;

namespace Zagejmi.Server.Domain.Repository;

public interface IRepositoryPersonWrite
{
    void Add(Person person);

    void Update(Person person);
}