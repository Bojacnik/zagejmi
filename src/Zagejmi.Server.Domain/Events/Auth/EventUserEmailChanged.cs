using Zagejmi.Server.Domain.Auth;

namespace Zagejmi.Server.Domain.Events.Auth;

public sealed record EventUserEmailChanged : IDomainEvent<User, Guid>
{
    public Guid UserId { get; }
    public string NewEmail { get; }
    public DateTime Timestamp { get; }
    public EventTypeDomain EventType { get; }

    public Guid AggregateId => UserId;

    public EventUserEmailChanged(Guid userId, string newEmail)
    {
        UserId = userId;
        NewEmail = newEmail;
        Timestamp = DateTime.UtcNow;
        EventType = EventTypeDomain.UserEmailChanged;
    }
}
