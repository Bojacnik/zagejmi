namespace Zagejmi.Application.Commands.Person;

public record CommandPersonVerifyByAdmin(
    Guid PersonId,
    VerificationPersonal Verification
) : ICommandPerson;