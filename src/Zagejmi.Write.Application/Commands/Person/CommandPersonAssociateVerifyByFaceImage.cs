namespace Zagejmi.Write.Application.Commands.Person;

public record CommandPersonAssociateVerifyByFaceImage(
    Guid PersonId,
    VerificationFace VerificationFace
) : ICommandPerson;