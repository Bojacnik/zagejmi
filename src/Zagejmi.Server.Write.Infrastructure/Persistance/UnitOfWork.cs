using LanguageExt;
using Zagejmi.Server.Write.Application;
using Zagejmi.Server.Write.Application.EventStore;
using Zagejmi.Server.Write.Domain.Community.People;
using Zagejmi.Server.Write.Infrastructure.Ctx;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Server.Write.Infrastructure.Persistance;

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