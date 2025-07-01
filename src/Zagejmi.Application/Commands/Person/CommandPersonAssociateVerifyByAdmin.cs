using System.Net.Mail;

namespace Zagejmi.Application.Commands.Person;

public record CommandPersonAssociateVerifyByAdmin(
    Guid PersonId,
    VerificationPersonal Verification 
) : ICommandPerson;