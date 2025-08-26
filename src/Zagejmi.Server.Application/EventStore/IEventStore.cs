using LanguageExt;
using System.Threading.Tasks;
using Zagejmi.Server.Domain.Entity;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Server.Application.EventStore;

public interface IEventStore<TAggregateRoot, TAggregateRootId>
    where TAggregateRoot : Aggregate<TAggregateRoot, TAggregateRootId>
    where TAggregateRootId : notnull
{
    Task<Either<FailureEventStore, Unit>> SaveAsync(TAggregateRoot aggregate, CancellationToken cancellationToken = default);

    Task<Option<TAggregateRoot>> LoadAggregateAsync(TAggregateRootId id, CancellationToken cancellationToken = default);
}
