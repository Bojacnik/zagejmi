namespace Zagejmi.Server.Application.Commands.Person;

public record CommandPersonAssociateVerifyByAdmin(
    Guid PersonId,
    VerificationPersonal Verification 
) : ICommandPerson;