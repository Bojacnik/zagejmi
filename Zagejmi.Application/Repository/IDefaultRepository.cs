using Zagejmi.Domain;
using Zagejmi.Domain.Events;

namespace Zagejmi.Application.Repository;

public interface IDefaultRepository<TEntity, TEvent>
    where TEntity : Entity<TEvent> where TEvent : IDomainEvent
{
    Task<ICollection<TEntity>> GetAllAsync(CancellationToken cancellationToken);
    Task<TEntity> GetByIdAsync(ulong id, CancellationToken cancellationToken);
    void AddAsync(TEntity entity, CancellationToken cancellationToken);
    void UpdateAsync(TEntity oldEntity, TEntity newEntity, CancellationToken cancellationToken);
    void DeleteAsync(ulong id, CancellationToken cancellationToken);
    Task<bool> FlushAsync(CancellationToken cancellationToken);
}