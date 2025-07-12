using LanguageExt;
using SharedKernel;
using SharedKernel.Failures;
using SharedKernel.Outbox;
using Zagejmi.Domain.Community.User;

namespace Zagejmi.Application;

/// <summary>
/// Represents a unit of work that can be used to save all changes
/// made within a single business transaction.
/// </summary>
public interface IUnitOfWork
{
    public Task<Either<FailureEventStore, Unit>> AddEventToEventStore(IDomainEvent<Person, Guid> @event);

    public Task<Either<FailureDatabase, Unit>> AddUpdateOperationWriteDatabase(
        Func<Task<Either<FailureDatabase, Unit>>> funcAsync);

    /// <summary>
    /// Saves all changes made in this unit of work to the underlying database.
    /// This includes business entities and any outbox events.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    Task<Either<FailureDatabase, Unit>> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an outbox event to be saved within the current transaction.
    /// </summary>
    Task<Either<FailureMessageBus, Unit>> AddOutboxEventAsync(OutboxEvent outboxEvent,
        CancellationToken cancellationToken = default);
}