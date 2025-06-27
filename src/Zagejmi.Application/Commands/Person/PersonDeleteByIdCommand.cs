using MediatR;
using Zagejmi.Domain.Events.People;

namespace Zagejmi.Application.Commands.Person;

public record PersonDeleteByIdCommand(ulong Id)
    : IRequest<PersonDeletedEvent>;