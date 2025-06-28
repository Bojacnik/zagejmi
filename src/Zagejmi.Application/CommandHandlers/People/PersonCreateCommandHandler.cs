using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.Logging;
using Serilog;
using Zagejmi.Application.Commands.Person;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.Domain.Community.User;
using Zagejmi.Domain.Community.User.Associate;
using Zagejmi.Domain.Events;
using Zagejmi.Domain.Events.EventStore;
using Zagejmi.Domain.Events.People;
using Zagejmi.Domain.Repository;

namespace Zagejmi.Application.CommandHandlers.People;

public class PersonCreateCommandHandler : IConsumer<PersonCreateCommand>
{
    public async Task Consume(ConsumeContext<PersonCreateCommand> context)
    {
        PersonCreateCommand command = context.Message;

        GoinWallet goinWallet = command is PersonCReateNewWithWalletCommand
            ? ((PersonCReateNewWithWalletCommand)command).GoinWallet
            : new GoinWallet([]);
        var person = new Person(
            new PersonalInfo(
                command.MailAddress,
                command.UserName,
                command.FirstName,
                command.LastName,
                command.BirthDate,
                command.Gender
            ),
            new PersonalStatistics(
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0
            ),
            goinWallet,
            PersonType.Customer,
            null,
            0
        );

        var @event = new PersonCreatedEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            0,
            Guid.Parse(person.Id.ToString()),
            nameof(PersonCreatedEvent)
        )
        {
            Email = person.PersonalInfo.MailAddress!,
            UserName = person.PersonalInfo.UserName!,
            FirstName = person.PersonalInfo.FirstName!,
            LastName = person.PersonalInfo.LastName!,
        };

        try
        {
            await context.Publish(@event, context.CancellationToken);
        }
        catch (OperationCanceledException e)
        {
            Log.Error(e, "Operation was canceled");
            throw;
        }
    }
}