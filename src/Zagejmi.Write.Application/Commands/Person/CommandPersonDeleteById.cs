namespace Zagejmi.Write.Application.Commands.Person;

public record CommandPersonDeleteById(Guid Id) : ICommandPerson;