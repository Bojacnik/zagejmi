using LanguageExt;
using Zagejmi.Contracts.Failures;
using Zagejmi.Write.Domain.Abstractions;
using Zagejmi.Write.Domain.Events;

namespace Zagejmi.Write.Domain;

public interface IEventBusProducer<TAggregateRoot, TAggregateRootId>
    where TAggregateRoot : Aggregate<TAggregateRoot, TAggregateRootId>
    where TAggregateRootId : notnull
{
    public Task<Either<Failure, Unit>> SendAsync(
        IDomainEvent<TAggregateRoot, TAggregateRootId> @event,
        CancellationToken cancellationToken);
}