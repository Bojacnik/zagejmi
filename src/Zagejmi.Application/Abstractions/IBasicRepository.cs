using System.Threading;
using System.Threading.Tasks;

namespace Zagejmi.Application.Abstractions;

/// <summary>
///     Defines a generic repository interface for performing basic CRUD operations on entities of type
///     <typeparamref name="TEntity" />.
/// </summary>
/// <typeparam name="TEntity">The type of the entity that the repository will manage. Must be a reference type.</typeparam>
/// <typeparam name="TEntityId">The type of the unique identifier for the entity. Must be a struct.</typeparam>
public interface IBasicRepository<TEntity, TEntityId>
    where TEntity : class
    where TEntityId : struct
{
    /// <summary>
    ///     Asynchronously retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the retrieved entity, or null if
    ///     not found.
    /// </returns>
    Task<TEntity?> GetByIdAsync(
        TEntityId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<TEntityId> AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously removes an entity from the repository.
    /// </summary>
    /// <param name="entityId">The identifier of the entity to remove.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RemoveAsync(
        TEntityId entityId,
        CancellationToken cancellationToken = default);
}