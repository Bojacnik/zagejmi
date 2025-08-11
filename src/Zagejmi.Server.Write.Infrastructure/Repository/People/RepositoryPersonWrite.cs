using Zagejmi.Server.Write.Domain.Community.People;
using Zagejmi.Server.Write.Domain.Repository;
using Zagejmi.Server.Write.Infrastructure.Ctx;

namespace Zagejmi.Server.Write.Infrastructure.Repository.People;

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