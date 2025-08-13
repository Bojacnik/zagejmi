using Zagejmi.Server.Domain.Auth;

namespace Zagejmi.Server.Domain.Events.Auth;

public sealed class UserCreated : IDomainEvent<User, Guid>
{
    public Guid UserId { get; }
    public string Username { get; }
    public Password Password { get; }
    public string Email { get; }
    public DateTime Timestamp { get; }
    public EventTypeDomain EventType { get; }

    public UserCreated(Guid userId, string username, Password password, string email)
    {
        UserId = userId;
        Username = username;
        Password = password;
        Email = email;
        Timestamp = DateTime.UtcNow;
        EventType = EventTypeDomain.UserCreated;
    }

    public User Apply(User aggregate)
    {
        aggregate.Id = UserId;
        aggregate.Username = Username;
        aggregate.Password = Password;
        aggregate.Email = Email;
        
        return aggregate;
    }
}