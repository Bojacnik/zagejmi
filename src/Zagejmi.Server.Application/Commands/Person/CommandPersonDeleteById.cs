namespace Zagejmi.Server.Application.Commands.Person;

public record CommandPersonDeleteById(Guid Id) : ICommandPerson;