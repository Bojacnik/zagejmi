using MassTransit;
using Microsoft.Extensions.Logging;
using Zagejmi.Server.Write.Application.Commands.Person;
using Zagejmi.Server.Write.Domain.Community.People;
using Zagejmi.Server.Write.Domain.Repository;

namespace Zagejmi.Server.Write.Application.CommandHandlers.People;

public sealed record HandlerPersonCreate(
    IEventStoreRepository<Person, Guid> EventStoreRepository,
    ILogger<HandlerPersonCreate> Logger) : IConsumer<CommandPersonCreate>
{
    public Task Consume(ConsumeContext<CommandPersonCreate> context)
    {
        throw new NotImplementedException();
    }
}