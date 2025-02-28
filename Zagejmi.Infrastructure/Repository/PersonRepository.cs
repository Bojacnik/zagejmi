using Zagejmi.Application.Repository;
using Zagejmi.Domain.Community.User;
using Zagejmi.Infrastructure.Ctx;
using Zagejmi.Infrastructure.Models;

namespace Zagejmi.Infrastructure.Repository;

public class PersonRepository(ZagejmiContext dbContext) : IPersonRepository
{
    private ZagejmiContext _dbContext = dbContext;
    
    public Task<ICollection<Person>> GetAllAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Person> GetByIdAsync(ulong id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void AddAsync(Person entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void UpdateAsync(Person oldEntity, Person newEntity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void DeleteAsync(ulong id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> FlushAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Person> GetByEmailAsync(string email)
    {
        throw new NotImplementedException();
    }

    public Task<Person> GetByUsernameAsync(string username)
    {
        throw new NotImplementedException();
    }
}