using Zagejmi.Domain.Community.People.Person;
using Zagejmi.Domain.Repository;
using Zagejmi.Infrastructure.Ctx;

namespace Zagejmi.Infrastructure.Repository.People;

public class RepositoryPersonWrite(ZagejmiContext dbContext) : IRepositoryPersonWrite
{
    public void Add(Person person)
    {
        throw new NotImplementedException();
    }

    public void Update(Person person)
    {
        throw new NotImplementedException();
    }

    private ZagejmiContext _dbContext = dbContext;
}