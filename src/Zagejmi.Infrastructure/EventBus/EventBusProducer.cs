using LanguageExt;
using SharedKernel;
using SharedKernel.Failures;
using Zagejmi.Domain.Community.People;

namespace Zagejmi.Infrastructure.EventBus;

public class EventBusProducer : IEventBusProducer<Person, Guid>
{
    public Task<Either<Failure, Unit>> SendAsync(IDomainEvent<Person, Guid> @event, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}