using System.Net.Mail;
using MediatR;
using Zagejmi.Application.Events.People;
using Zagejmi.Domain.Community.User;

namespace Zagejmi.Application.Commands.Person;

public record CreatePersonCommand(
    MailAddress MailAddress,
    string UserName,
    string FirstName,
    string LastName,
    DateTime BirthDate,
    Gender Gender) : IRequest<PersonCreatedEvent>;