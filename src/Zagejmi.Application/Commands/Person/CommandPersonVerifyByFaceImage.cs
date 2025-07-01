using System.Net.Mail;

namespace Zagejmi.Application.Commands.Person;

public record CommandPersonVerifyByFaceImage(
    Guid PersonId,
    VerificationFace VerificationFace
) : ICommandPerson;