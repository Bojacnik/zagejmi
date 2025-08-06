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

public sealed record HandlerPersonCreate(
    IEventStoreRepository<Person, Guid> EventStoreRepository,
    ILogger<HandlerPersonCreate> Logger) : IConsumer<CommandPersonCreate>
{
    public Task Consume(ConsumeContext<CommandPersonCreate> context)
    {
        throw new NotImplementedException();
    }
}