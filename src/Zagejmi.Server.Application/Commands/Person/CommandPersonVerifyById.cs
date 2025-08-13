namespace Zagejmi.Server.Application.Commands.Person;

public record CommandPersonVerifyById(
    Guid PersonId,
    VerificationId VerificationId
) : ICommandPerson;