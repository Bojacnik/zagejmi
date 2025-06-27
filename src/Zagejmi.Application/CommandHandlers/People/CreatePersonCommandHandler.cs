using MediatR;
using Zagejmi.Application.Commands.Person;
using Zagejmi.Domain.Events.EventStore;
using Zagejmi.Domain.Events.People;
using Zagejmi.Domain.Repository;

namespace Zagejmi.Application.CommandHandlers;

public class CreatePersonCommandHandler(
    IEventStore eventStore,
    IPersonRepository repository)
    : IRequestHandler<PersonCreateCommand, PersonCreatedEvent>
{
    public async Task<PersonCreatedEvent> Handle(PersonCreateCommand request, CancellationToken cancellationToken)
    {
        var @event = new PersonCreatedEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            0,
            // TODO: Should be Person Aggregate Id not random???
            Guid.NewGuid(),
            nameof(PersonCreatedEvent)
        )
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.MailAddress,
            UserName = request.UserName
        };

        await eventStore.SaveEventAsync(@event, cancellationToken);
        await repository.FlushAsync(cancellationToken);

        return @event;
    }
}