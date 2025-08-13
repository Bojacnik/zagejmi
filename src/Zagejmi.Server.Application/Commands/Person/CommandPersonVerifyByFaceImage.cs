namespace Zagejmi.Server.Application.Commands.Person;

public record CommandPersonVerifyByFaceImage(
    Guid PersonId,
    VerificationFace VerificationFace
) : ICommandPerson;