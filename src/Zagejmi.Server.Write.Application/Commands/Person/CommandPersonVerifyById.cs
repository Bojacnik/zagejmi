namespace Zagejmi.Server.Write.Application.Commands.Person;

public record CommandPersonVerifyById(
    Guid personID,
    VerificationId VerificationId
) : ICommandPerson;