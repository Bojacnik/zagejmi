using LanguageExt;
using Zagejmi.Server.Domain.Entity;
using Zagejmi.Server.Domain.Events;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Server.Domain;

public interface IEventBusProducer<TAggregateRoot, TAggregateRootId>
    where TAggregateRoot : AggregateRoot<TAggregateRoot, TAggregateRootId>
    where TAggregateRootId : notnull
{
    public Task<Either<Failure, Unit>> SendAsync(
        IDomainEvent<TAggregateRoot, TAggregateRootId> @event,
        CancellationToken cancellationToken);
}