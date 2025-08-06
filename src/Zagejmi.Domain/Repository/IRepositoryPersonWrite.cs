using Zagejmi.Domain.Community.People;

namespace Zagejmi.Domain.Repository;

public interface IRepositoryPersonWrite
{
    void Add(Person person);

    void Update(Person person);
}