using Zagejmi.Domain.Community.User;

namespace Zagejmi.Domain.Events.People;

public record PersonUpdatedEvent(
    Guid EventId,
    DateTime Timestamp,
    ulong Version,
    Guid AggregateId,
    string EventType) : IPersonEvent
{
    public ulong PersonId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public DateTime BirthDate { get; init; }
    public Gender Gender { get; init; }
}