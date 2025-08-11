namespace Zagejmi.Server.Write.Application.Commands.Person;

public record CommandPersonVerifyByAdmin(
    Guid PersonId,
    VerificationPersonal Verification
) : ICommandPerson;