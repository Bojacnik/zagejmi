using MediatR;
using Zagejmi.Domain.Events.People;

namespace Zagejmi.Application.Commands.Person;

public record PersonUpdateCommand(
    Domain.Community.User.Person OldPerson,
    Domain.Community.User.Person NewPerson)
    : IRequest<PersonUpdatedEvent>;