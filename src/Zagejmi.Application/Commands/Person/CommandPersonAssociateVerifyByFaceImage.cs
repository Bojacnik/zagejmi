using System.Net.Mail;

namespace Zagejmi.Application.Commands.Person;

public record CommandPersonAssociateVerifyByFaceImage(
    Guid PersonId,
    VerificationFace VerificationFace
) : ICommandPerson;