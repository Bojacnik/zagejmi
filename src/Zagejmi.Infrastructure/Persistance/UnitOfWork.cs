using SharedKernel.Outbox;
using Zagejmi.Application;
using Zagejmi.Infrastructure.Ctx;

namespace Zagejmi.Infrastructure.Persistance;

public class UnitOfWork : IUnitOfWork
{
    private readonly ZagejmiContext _dbContext;

    public UnitOfWork(ZagejmiContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddOutboxEventAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken = default)
    {
        return _dbContext.OutboxEvents.AddAsync(outboxEvent, cancellationToken).AsTask();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}