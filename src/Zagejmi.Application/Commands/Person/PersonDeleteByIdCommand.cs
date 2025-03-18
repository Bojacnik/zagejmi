using MediatR;
using Zagejmi.Application.Events.People;

namespace Zagejmi.Application.Commands.Person;

public record PersonDeleteByIdCommand(ulong Id)
    : IRequest<PersonDeletedEvent>;