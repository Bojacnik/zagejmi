namespace Zagejmi.Application.Commands.Person;

public record CommandPersonUpdate(
    Domain.Community.User.Person OldPerson,
    Domain.Community.User.Person NewPerson);