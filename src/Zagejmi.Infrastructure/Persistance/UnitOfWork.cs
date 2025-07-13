using AnyMapper;
using LanguageExt;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Polly;
using Serilog;
using SharedKernel;
using SharedKernel.Failures;
using SharedKernel.Outbox;
using Zagejmi.Application;
using Zagejmi.Domain.Community.People;
using Zagejmi.Domain.Events.EventStore;
using Zagejmi.Infrastructure.Ctx;

namespace Zagejmi.Infrastructure.Persistance;

public class UnitOfWork : IUnitOfWork
{
    private readonly ZagejmiContext _dbContext;
    private readonly IEventStore<Person, Guid> _eventStore;

    public UnitOfWork(ZagejmiContext dbContext, IEventStore<Person, Guid> eventStore)
    {
        _dbContext = dbContext;
        _eventStore = eventStore;
    }

    public async Task<Either<Failure, Unit>> ExecuteAsync(
        IDomainEvent<Person, Guid> @event,
        Func<Task<Either<FailureDatabase, Unit>>> addUpdateToWriteDatabaseOperation,
        CancellationToken cancellationToken = default)
    {
        Either<FailureEventStore, Unit> additionToEventStoreResult = await AddEventToEventStore(@event);

        additionToEventStoreResult.IfLeft(failure => Log.Error(
            "Could not add event with timestamp {e} to event store because {message}",
            @event.Timestamp, failure.Message));

        Either<FailureDatabase, Unit> additionToWriteDatabase = await AddUpdateOperationWriteDatabase(
            addUpdateToWriteDatabaseOperation
        );

        additionToWriteDatabase.IfLeft(failure =>
            Log.Error("Could not add to write database because {message}", failure.Message));

        Either<FailureDatabase, Unit> saveChangesToWriteDatabaseResult = await SaveChangesAsync(cancellationToken);

        saveChangesToWriteDatabaseResult.IfLeft(failure => Log.Error(
            "Could not save to write database because {e}", failure.Message));

        Either<FailureMessageBus, Unit> saveChangesToOutbox =
            await AddOutboxEventAsync(Mapper.Map<OutboxEvent>(@event), cancellationToken);

        saveChangesToOutbox.IfLeft(failure =>
            Log.Error("Could not save to outbox because {e}", failure.Message)
        );

        return Unit.Default;
    }

    public async Task<Either<FailureEventStore, Unit>> AddEventToEventStore(IDomainEvent<Person, Guid> @event)
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        return await _eventStore.SaveEventAsync(@event, cts.Token);
    }

    public async Task<Either<FailureDatabase, Unit>> AddUpdateOperationWriteDatabase(
        Func<Task<Either<FailureDatabase, Unit>>> funcAsync)
    {
        return await funcAsync();
    }

    public async Task<Either<FailureDatabase, Unit>> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int resultAsInt = await _dbContext.SaveChangesAsync(cancellationToken);
        return MapDatabaseCodeToEither(resultAsInt);
    }

    public async Task<Either<FailureMessageBus, Unit>> AddOutboxEventAsync(OutboxEvent outboxEvent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            EntityEntry<OutboxEvent> result =
                await _dbContext.OutboxEvents.AddAsync(outboxEvent, cancellationToken).AsTask();
        }
        catch (OperationCanceledException e)
        {
            return new FailureMessageBusOperationCancelled("Adding to outbox events cancelled");
        }

        return Unit.Default;
    }

    private static Either<FailureDatabase, Unit> MapDatabaseCodeToEither(int failure)
    {
        return failure switch
        {
            -1 => new FailureDatabaseWrite("Failed to write to the write database!"),
            _ => Unit.Default
        };
    }
}