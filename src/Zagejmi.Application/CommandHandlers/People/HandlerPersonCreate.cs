using LanguageExt;
using MassTransit;
using Polly;
using Polly.Retry;
using Serilog;
using SharedKernel;
using SharedKernel.Failures;
using Zagejmi.Application.Commands.Person;
using Zagejmi.Domain.Community.User;
using Zagejmi.Domain.Events.EventStore;
using Zagejmi.Domain.Events.People;
using Zagejmi.Domain.Repository;
using Gender = Zagejmi.Application.Commands.Person.Gender;
using PersonType = Zagejmi.Domain.Community.User.PersonType;

namespace Zagejmi.Application.CommandHandlers.People;

public class HandlerPersonCreate : IConsumer<CommandPersonCreate>
{
    public HandlerPersonCreate(IRepositoryPersonWrite repository, IEventStore eventStore, IEventBusProducer eventBusProducer)
    {
        _repository = repository;
        _eventStore = eventStore;
        _eventBusProducer = eventBusProducer;
    }

    public async Task Consume(ConsumeContext<CommandPersonCreate> context)
    {
        CommandPersonCreate command = context.Message;
        // Use the CancellationToken from the context for better integration with the host
        var cancellationToken = context.CancellationToken;

        var info = new PersonalInformation(
            command.MailAddress,
            command.UserName,
            command.FirstName,
            command.LastName,
            command.BirthDate,
            command.Gender switch
            {
                Gender.Unknown => Domain.Community.User.Gender.Unknown,
                Gender.Male => Domain.Community.User.Gender.Male,
                Gender.Female => Domain.Community.User.Gender.Female,
                Gender.Other => Domain.Community.User.Gender.Other,
                _ => throw new ArgumentOutOfRangeException(nameof(command.Gender), "Invalid gender specified.")
            }
        );

        var stats = new PersonalStatistics(0, 0, 0, 0, 0, 0, 0, 0, 0);

        var person = new Person(
            Guid.NewGuid(),
            PersonType.Customer,
            info,
            stats,
            [],
            null
        );

        Either<Failure, Unit> resultOfCreate = await _repository.CreatePerson(person, cancellationToken);

        await resultOfCreate.Match(
            Right: async _ => await OnPersonCreatedSuccessfully(person, cancellationToken),
            Left: failure =>
            {
                Log.Error("Failed to create person: {Failure}", failure.Message);
                return Task.CompletedTask;
            });
    }

    private async Task OnPersonCreatedSuccessfully(Person person, CancellationToken cancellationToken)
    {
        Log.Information("Person created successfully {Person}", person.ToString());

        var @event = new EventPersonCreated(
            Guid.NewGuid(),
            DateTime.UtcNow, // Use UtcNow for consistency
            1,
            person.Id,
            "create")
        {
            FirstName = person.PersonalInformation.FirstName!,
            LastName = person.PersonalInformation.LastName!,
            UserName = person.PersonalInformation.UserName!,
            Email = person.PersonalInformation.MailAddress!,
            BirthDate = person.PersonalInformation.BirthDay,
            Gender = person.PersonalInformation.Gender,
            PersonType = person.PersonType,
        };

        try
        {
            await _eventStore.SaveEventAsync(@event, cancellationToken);

            AsyncRetryPolicy? retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(10),
                    onRetry: (exception, timeSpan, retryCount, ctx) =>
                    {
                        Log.Warning(exception,
                            "Failed to send event to bus. Retrying in {timeSpan}. Attempt {retryCount}/3", timeSpan,
                            retryCount);
                    });

            await retryPolicy.ExecuteAsync(
                async token =>
                {
                    return (await _eventBusProducer.SendAsync(@event, token)).Match(
                        Right: _ => Task.CompletedTask,
                        Left: failure => Task.FromException(new Exception("Failed to send event to bus."))
                    );
                },
                cancellationToken);

            Log.Information("EventPersonCreated sent to event bus successfully for PersonId {PersonId}", person.Id);
        }
        catch (Exception ex)
        {
            Log.Error(ex,
                "An unrecoverable error occurred while processing post-creation events for PersonId {PersonId}",
                person.Id);
        }
    }

    private readonly IRepositoryPersonWrite _repository;
    private readonly IEventStore _eventStore;
    private readonly IEventBusProducer _eventBusProducer;
}