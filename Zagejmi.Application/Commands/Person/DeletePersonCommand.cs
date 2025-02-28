using MediatR;
using Zagejmi.Application.Events;
using Zagejmi.Application.Events.People;

namespace Zagejmi.Application.Commands.Person;

public record DeletePersonByIdCommand(ulong Id)
    : IRequest<PersonDeletedEvent>;