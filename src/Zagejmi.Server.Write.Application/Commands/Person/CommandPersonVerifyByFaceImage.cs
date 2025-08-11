namespace Zagejmi.Server.Write.Application.Commands.Person;

public record CommandPersonVerifyByFaceImage(
    Guid PersonId,
    VerificationFace VerificationFace
) : ICommandPerson;