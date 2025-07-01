using LanguageExt;
using SharedKernel.Failures;

namespace SharedKernel;

public interface IEventBusProducer
{
    public Task<Either<Failure, Unit>> SendAsync(IDomainEvent @event, CancellationToken cancellationToken);
}