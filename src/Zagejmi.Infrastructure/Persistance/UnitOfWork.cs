using LanguageExt;
using Zagejmi.Application;
using Zagejmi.Application.EventStore;
using Zagejmi.Domain.Community.People.Person;
using Zagejmi.Infrastructure.Ctx;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Infrastructure.Persistance;

public class UnitOfWork(ZagejmiContext dbContext, IEventStore<Person, Guid> eventStore)
    : IUnitOfWork
{
    private readonly ZagejmiContext _dbContext = dbContext;
    private readonly IEventStore<Person, Guid> _eventStore = eventStore;
    public Task<Either<Failure, Unit>> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}