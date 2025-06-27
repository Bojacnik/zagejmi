using SharedKernel;
using Zagejmi.Domain.Events;

namespace Zagejmi.Domain.Repository;

public interface IDefaultRepository<TEntity, TEvent>
    where TEntity : Entity<TEvent> where TEvent : IDomainEvent
{
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken);
    Task<TEntity> GetByIdAsync(ulong id, CancellationToken cancellationToken);
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken);
    Task<TEntity> UpdateAsync(TEntity oldEntity, TEntity newEntity, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken);
    Task<bool> FlushAsync(CancellationToken cancellationToken);
}