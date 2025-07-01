using System.Net.Mail;

namespace Zagejmi.Application.Commands.Person;

public record CommandPersonAssociateVerifyById(
    Guid personId,
    VerificationId VerificationId
) : ICommandPerson;