using LanguageExt;
using Zagejmi.Server.Write.Domain.Entity;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Server.Write.Domain;

public interface IEventBusProducer<TAggregateRoot, TAggregateRootId>
    where TAggregateRoot : AggregateRoot<TAggregateRoot, TAggregateRootId>
    where TAggregateRootId : notnull
{
    public Task<Either<Failure, Unit>> SendAsync(
        IDomainEvent<TAggregateRoot, TAggregateRootId> @event,
        CancellationToken cancellationToken);
}