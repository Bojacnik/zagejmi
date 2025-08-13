namespace Zagejmi.Server.Application.Commands.Person;

public record CommandPersonVerifyByAdmin(
    Guid PersonId,
    VerificationPersonal Verification
) : ICommandPerson;