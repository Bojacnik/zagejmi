namespace Zagejmi.Server.Write.Application.Commands.Person;

public record CommandPersonAssociateVerifyById(
    Guid personId,
    VerificationId VerificationId
) : ICommandPerson;