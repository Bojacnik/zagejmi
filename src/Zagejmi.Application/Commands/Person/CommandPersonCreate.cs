using System.Net.Mail;

namespace Zagejmi.Application.Commands.Person;

public abstract record CommandPersonCreate(
    MailAddress MailAddress,
    string UserName,
    string FirstName,
    string LastName,
    DateTime BirthDate,
    Gender Gender
) : ICommandPerson;