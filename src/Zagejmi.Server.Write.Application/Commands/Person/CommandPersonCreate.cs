using System.Net.Mail;

namespace Zagejmi.Server.Write.Application.Commands.Person;

public abstract record CommandPersonCreate(
    MailAddress MailAddress,
    string UserName,
    string Password,
    string FirstName,
    string LastName,
    DateTime BirthDate,
    Gender Gender
) : ICommandPerson;