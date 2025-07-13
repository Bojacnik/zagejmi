using System.Text.Json;
using LanguageExt;
using MassTransit;
using Serilog;
using SharedKernel;
using SharedKernel.Failures;
using SharedKernel.Outbox;
using Zagejmi.Application.Commands.Person;
using Zagejmi.Domain.Community.People;
using Zagejmi.Domain.Events.People;
using Zagejmi.Domain.Repository; // Import the IUnitOfWork namespace
using Gender = Zagejmi.Application.Commands.Person.Gender;
using PersonType = Zagejmi.Domain.Community.People.PersonType;

namespace Zagejmi.Application.CommandHandlers.People;

public sealed record HandlerPersonCreate : IConsumer<CommandPersonCreate>
{
    // Inject the Unit of Work and the Repository
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepositoryPersonWrite _repository;

    public HandlerPersonCreate(IUnitOfWork unitOfWork, IRepositoryPersonWrite repository)
    {
        _unitOfWork = unitOfWork;
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<CommandPersonCreate> context)
    {
        CommandPersonCreate command = context.Message;
        CancellationToken cancellationToken = context.CancellationToken;

        // 1. Create the domain entities (This part is unchanged)
        PersonalInformation info = new PersonalInformation(
            command.MailAddress,
            command.UserName,
            command.FirstName,
            command.LastName,
            command.BirthDate,
            command.Gender switch
            {
                Gender.Unknown => Domain.Community.People.Gender.Unknown,
                Gender.Male => Domain.Community.People.Gender.Male,
                Gender.Female => Domain.Community.People.Gender.Female,
                Gender.Other => Domain.Community.People.Gender.Other,
                _ => throw new ArgumentOutOfRangeException(nameof(command.Gender), "Invalid gender specified.")
            }
        );
        PersonalStatistics stats =
            new PersonalStatistics(0, 0, 0, 0, 0, 0, 0, 0, 0);
        Guid personGuid = Guid.NewGuid();
        Person person = new Person(personGuid, PersonType.Customer, info, stats, [], null);

        // 2. Add the new Person via the repository.
        // The repository uses the same DbContext instance (managed by DI),
        // so this adds the person to the Unit of Work's change tracker.
        Either<Failure, Unit> resultOfCreate = await _repository.CreatePerson(person, cancellationToken);

        if (resultOfCreate.IsLeft)
        {
            Log.Error("Failed to create person: {Failure}", ((Failure)resultOfCreate).Message);
            return;
        }

        // 3. Create the domain event
        EventPersonCreated personCreatedEvent = new EventPersonCreated(
            DateTime.UtcNow,
            EventTypeDomain.PersonCreated,
            personGuid)
        {
            AggregateId = personGuid,
            FirstName = person.PersonalInformation.FirstName!,
            LastName = person.PersonalInformation.LastName!,
            UserName = person.PersonalInformation.UserName!,
            Email = person.PersonalInformation.MailAddress!,
            BirthDate = person.PersonalInformation.BirthDay,
            Gender = person.PersonalInformation.Gender,
            PersonType = person.PersonType,
        };

        // 4. Create the OutboxEvent
        OutboxEvent outboxEvent = new OutboxEvent
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow,
            EventType = personCreatedEvent.EventType,
            Content = JsonSerializer.Serialize(personCreatedEvent, personCreatedEvent.GetType())
        };

        // 5. Add the OutboxEvent via the Unit of Work
        await _unitOfWork.AddOutboxEventAsync(outboxEvent, cancellationToken);

        // 6. Commit the transaction via the Unit of Work
        // This single call atomically saves the Person and the OutboxEvent.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        Log.Information("Person {PersonId} and OutboxEvent {EventId} saved to database transactionally.", person.Id,
            outboxEvent.Id);
    }
}