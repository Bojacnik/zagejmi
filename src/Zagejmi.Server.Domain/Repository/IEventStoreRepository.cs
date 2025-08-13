using Zagejmi.Server.Domain.Entity;

namespace Zagejmi.Server.Domain.Repository;

/// <summary>
/// Represents a repository for storing and retrieving aggregates as a stream of events.
/// This is the "write" repository in an Event Sourcing architecture.
/// </summary>
/// <typeparam name="TAggregateRoot">The type of the aggregate root.</typeparam>
/// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
public interface IEventStoreRepository<TAggregateRoot, in TId> 
    where TAggregateRoot : AggregateRoot<TAggregateRoot, TId>
    where TId : notnull
{
    Task SaveAsync(TAggregateRoot aggregate, CancellationToken cancellationToken = default);

    Task<TAggregateRoot?> LoadAsync(TId aggregateId, CancellationToken cancellationToken = default);
}