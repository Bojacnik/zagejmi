namespace Zagejmi.Write.Domain.Auth.Events;

public sealed class EventUserUpdated : IDomainEvent<User, Guid>
{
    public EventUserUpdated(Guid userId, string username, string password, string email)
    {
        this.UserId = userId;
        this.Username = username;
        this.Password = password;
        this.Email = email;
        this.Timestamp = DateTime.UtcNow;
        this.EventType = EventTypeDomain.UserUpdated;
    }

    public Guid UserId { get; }

    public string Username { get; }

    public string Password { get; }

    public string Email { get; }

    public DateTime Timestamp { get; }

    public EventTypeDomain EventType { get; }

    public Guid AggregateId => this.UserId;
}