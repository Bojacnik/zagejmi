namespace Zagejmi.Server.Write.Application.Commands.Person;

public record CommandPersonAssociateVerifyByAdmin(
    Guid PersonId,
    VerificationPersonal Verification 
) : ICommandPerson;