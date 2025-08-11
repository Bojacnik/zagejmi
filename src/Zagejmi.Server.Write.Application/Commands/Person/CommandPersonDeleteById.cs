namespace Zagejmi.Server.Write.Application.Commands.Person;

public record CommandPersonDeleteById(Guid Id) : ICommandPerson;