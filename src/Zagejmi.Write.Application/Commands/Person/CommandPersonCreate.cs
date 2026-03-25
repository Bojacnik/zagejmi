using System.Net.Mail;

namespace Zagejmi.Write.Application.Commands.Person;

public abstract record CommandPersonCreate(
    Guid UserId,
    MailAddress MailAddress,
    string UserName,
    string Password,
    string FirstName,
    string LastName,
    DateTime BirthDate,
    Gender Gender
) : ICommandPerson;