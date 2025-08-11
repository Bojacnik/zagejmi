using Zagejmi.Server.Write.Domain.Community.People;

namespace Zagejmi.Server.Write.Domain.Repository;

public interface IRepositoryPersonWrite
{
    void Add(Person person);

    void Update(Person person);
}