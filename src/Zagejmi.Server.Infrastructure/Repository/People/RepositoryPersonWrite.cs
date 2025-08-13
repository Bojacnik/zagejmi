using Zagejmi.Server.Domain.Community.People;
using Zagejmi.Server.Domain.Repository;
using Zagejmi.Server.Infrastructure.Ctx;

namespace Zagejmi.Server.Infrastructure.Repository.People;

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