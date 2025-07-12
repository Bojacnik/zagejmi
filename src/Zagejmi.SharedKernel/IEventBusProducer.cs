using LanguageExt;
using SharedKernel.Failures;

namespace SharedKernel;

public interface IEventBusProducer<TAggregateRoot, TAggregateRootId>
    where TAggregateRoot : AggregateRoot<TAggregateRoot, TAggregateRootId>
    where TAggregateRootId : notnull
{
    public Task<Either<Failure, Unit>> SendAsync(
        IDomainEvent<TAggregateRoot, TAggregateRootId> @event,
        CancellationToken cancellationToken);
}