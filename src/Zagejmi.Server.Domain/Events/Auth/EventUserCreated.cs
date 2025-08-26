using Zagejmi.Server.Domain.Auth;

namespace Zagejmi.Server.Domain.Events.Auth;

public sealed record EventUserCreated : IDomainEvent<User, Guid>
{
    public Guid UserId { get; }
    public string Username { get; }
    public string Password { get; }
    public string Salt { get; }
    public string Email { get; }
    public DateTime Timestamp { get; }
    public EventTypeDomain EventType { get; }

    public Guid AggregateId => UserId;

    public EventUserCreated(Guid userId, string username, string password, string salt, string email)
    {
        UserId = userId;
        Username = username;
        Password = password;
        Salt = salt;
        Email = email;
        Timestamp = DateTime.UtcNow;
        EventType = EventTypeDomain.UserCreated;
    }
}
