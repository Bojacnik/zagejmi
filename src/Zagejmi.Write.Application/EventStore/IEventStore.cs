using LanguageExt;
using System.Threading.Tasks;
using Zagejmi.SharedKernel.Failures;
using Zagejmi.Write.Domain.Abstractions;

namespace Zagejmi.Write.Application.EventStore;

public interface IEventStore<TAggregateRoot, TAggregateRootId>
    where TAggregateRoot : Aggregate<TAggregateRoot, TAggregateRootId>
    where TAggregateRootId : notnull
{
    Task<Either<FailureEventStore, Unit>> SaveAsync(TAggregateRoot aggregate, CancellationToken cancellationToken = default);

    Task<Option<TAggregateRoot>> LoadAggregateAsync(TAggregateRootId id, CancellationToken cancellationToken = default);
}
