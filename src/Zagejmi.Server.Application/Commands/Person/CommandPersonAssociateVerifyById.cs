namespace Zagejmi.Server.Application.Commands.Person;

public record CommandPersonAssociateVerifyById(
    Guid PersonId,
    VerificationId VerificationId
) : ICommandPerson;