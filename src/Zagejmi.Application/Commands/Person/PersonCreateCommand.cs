using System.Net.Mail;
using MediatR;
using Zagejmi.Domain.Community.User;
using Zagejmi.Domain.Events.People;

namespace Zagejmi.Application.Commands.Person;

public record PersonCreateCommand(
    MailAddress MailAddress,
    string UserName,
    string FirstName,
    string LastName,
    DateTime BirthDate,
    Gender Gender) : IRequest<PersonCreatedEvent>;