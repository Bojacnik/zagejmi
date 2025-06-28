namespace Zagejmi.Application.Commands.Person;

public record PersonUpdateCommand(
    Domain.Community.User.Person OldPerson,
    Domain.Community.User.Person NewPerson);