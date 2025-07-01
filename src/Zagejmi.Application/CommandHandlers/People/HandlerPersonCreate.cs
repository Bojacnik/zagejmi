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

public class HandlerPersonCreate : IConsumer<CommandPersonCreate>
{
    public async Task Consume(ConsumeContext<CommandPersonCreate> context)
    {
        CommandPersonCreate command = context.Message;

        GoinWallet[] walletArray = [];
        GoinWallet goinWallet =
            command is CommandPersonCReateNewWithWallet wallet
                ? wallet.GoinWallet
                : new GoinWallet(1, []);
        var person = new Person(
            0,
            PersonType.Customer,
            new PersonalInformation(
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
                0
            ),
            walletArray,
            null
        );

        var @event = new PersonCreatedEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            0,
            Guid.Parse(person.Id.ToString()),
            nameof(PersonCreatedEvent)
        )
        {
            Email = person.PersonalInformation.MailAddress!,
            UserName = person.PersonalInformation.UserName!,
            FirstName = person.PersonalInformation.FirstName!,
            LastName = person.PersonalInformation.LastName!,
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