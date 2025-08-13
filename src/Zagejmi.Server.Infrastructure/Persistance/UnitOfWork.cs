using LanguageExt;
using Zagejmi.Server.Application;
using Zagejmi.Server.Application.EventStore;
using Zagejmi.Server.Domain.Community.People;
using Zagejmi.Server.Infrastructure.Ctx;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Server.Infrastructure.Persistance;

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