using SharedKernel.Outbox;

namespace Zagejmi.Application;

/// <summary>
/// Represents a unit of work that can be used to save all changes
/// made within a single business transaction.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Adds an outbox event to be saved within the current transaction.
    /// </summary>
    Task AddOutboxEventAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes made in this unit of work to the underlying database.
    /// This includes business entities and any outbox events.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}