using System.Net.Mail;

namespace Zagejmi.Application.Commands.Person;

public record CommandPersonVerifyById(
    Guid personID,
    VerificationId VerificationId
) : ICommandPerson;