using MassTransit;
using Microsoft.Extensions.Logging;
using Zagejmi.Application.Commands.Person;
using Zagejmi.Domain.Community.People;
using Zagejmi.Domain.Repository;

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