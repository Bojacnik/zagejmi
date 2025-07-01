using System.Net.Mail;
using Zagejmi.Domain.Community.User;

namespace Zagejmi.Domain.Events.People;

public record EventPersonCreated(
    Guid EventId,
    DateTime Timestamp,
    ulong Version,
    Guid AggregateId,
    string EventType)
    : IPersonEvent
{
    public ulong? Id { get; init; }
    public required string FirstName { get; init; }
    public required string UserName { get; init; }
    public required string LastName { get; init; }
    public required MailAddress Email { get; init; }
    public DateTime BirthDate { get; init; }
    public Gender Gender { get; init; }
    public required PersonType PersonType { get; init; }
}