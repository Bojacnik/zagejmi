using Zagejmi.Server.Domain.Auth;

namespace Zagejmi.Server.Domain.Events.Auth;

public sealed class EventUserUpdated : IDomainEvent<User, Guid>
{
    public Guid UserId { get; }
    public string Username { get; }
    public string Password { get; }
    public string Email { get; }
    public DateTime Timestamp { get; }
    public EventTypeDomain EventType { get; }

    public Guid AggregateId => UserId;

    public EventUserUpdated(Guid userId, string username, string password, string email)
    {
        UserId = userId;
        Username = username;
        Password = password;
        Email = email;
        Timestamp = DateTime.UtcNow;
        EventType = EventTypeDomain.UserUpdated;
    }
}
