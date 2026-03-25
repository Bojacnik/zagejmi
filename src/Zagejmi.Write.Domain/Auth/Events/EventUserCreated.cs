namespace Zagejmi.Write.Domain.Auth.Events;

public sealed record EventUserCreated : IDomainEvent<User, Guid>
{
    public EventUserCreated(Guid userId, string username, string password, string salt, string email)
    {
        this.UserId = userId;
        this.Username = username;
        this.Password = password;
        this.Salt = salt;
        this.Email = email;
        this.Timestamp = DateTime.UtcNow;
        this.EventType = EventTypeDomain.UserCreated;
    }

    public Guid UserId { get; }

    public string Username { get; }

    public string Password { get; }

    public string Salt { get; }

    public string Email { get; }

    public DateTime Timestamp { get; }

    public EventTypeDomain EventType { get; }

    public Guid AggregateId => this.UserId;
}