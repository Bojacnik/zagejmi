namespace Zagejmi.Write.Application.Commands.Person;

public record CommandPersonVerifyByAdmin(
    Guid PersonId,
    VerificationPersonal Verification
) : ICommandPerson;