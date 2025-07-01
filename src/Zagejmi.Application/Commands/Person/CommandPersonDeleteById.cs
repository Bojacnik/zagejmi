namespace Zagejmi.Application.Commands.Person;

public record CommandPersonDeleteById(Guid Id) : ICommandPerson;