using LanguageExt;
using MassTransit;
using Microsoft.Extensions.Logging;
using Serilog;
using Zagejmi.Application.Commands.Person;
using Zagejmi.Domain;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Auth.Hashing;
using Zagejmi.Domain.Community.People.Person;
using Zagejmi.Domain.Events.People;
using Zagejmi.Domain.Repository;
using Zagejmi.SharedKernel.Failures;
using Gender = Zagejmi.Application.Commands.Person.Gender;
using PersonType = Zagejmi.Domain.Community.People.Person.PersonType;

namespace Zagejmi.Application.CommandHandlers.People;

public sealed record HandlerPersonCreate : IConsumer<CommandPersonCreate>
{
    // TODO: Inject a real IHashHandler implementation
    private readonly IHashHandler _hashHandler = new StubHashHandler(); 
    private readonly IEventStoreRepository<Person, Guid> _eventStoreRepository;
    private readonly ILogger<HandlerPersonCreate> _logger;

    public HandlerPersonCreate(IEventStoreRepository<Person, Guid> eventStoreRepository, ILogger<HandlerPersonCreate> logger)
    {
        _eventStoreRepository = eventStoreRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CommandPersonCreate> context)
    {
        CommandPersonCreate command = context.Message;

        // 1. Create the domain entities (This part is unchanged)
        PersonalInformation info = new PersonalInformation(
            command.MailAddress,
            command.UserName,
            command.FirstName,
            command.LastName,
            command.BirthDate,
            command.Gender switch
            {
                Gender.Unknown => Domain.Community.People.Person.Gender.Unknown,
                Gender.Male => Domain.Community.People.Person.Gender.Male,
                Gender.Female => Domain.Community.People.Person.Gender.Female,
                Gender.Other => Domain.Community.People.Person.Gender.Other,
                _ => throw new ArgumentOutOfRangeException(nameof(command.Gender), "Invalid gender specified.")
            }
        );

        // Create the password securely
        string salt = command.UserName + command.MailAddress.Address; // Note: A cryptographically random salt is better
        Password password = Password.Create(command.Password, salt, _hashHandler);

        User user = new User(
            command.UserName,
            password,
            command.MailAddress.Address
        );

        // 2. Create the aggregate. This doesn't save anything, it just raises an event internally.
        Person person = Person.Create(Guid.NewGuid(), user, info);

        // 3. Persist the new event(s) to the event store (TimescaleDB).
        // This is our new "Unit of Work". It should be an atomic operation.
        await _eventStoreRepository.SaveAsync(person, context.CancellationToken);

        // 4. Publish the events to the message bus (RabbitMQ) for read-side projectors to consume.
        foreach (var domainEvent in person.GetUncommittedEvents())
        {
            // MassTransit can publish the raw event object directly.
            await context.Publish(domainEvent, context.CancellationToken);
        }

        _logger.LogInformation("Successfully created and published events for Person {PersonId}", person.Id);
    }
}

// A temporary stub for demonstration purposes. You would have a real implementation.
public class StubHashHandler : IHashHandler
{
    public string Hash(string password, string salt, HashType hashType) => $"hashed_{password}_with_{salt}";
}