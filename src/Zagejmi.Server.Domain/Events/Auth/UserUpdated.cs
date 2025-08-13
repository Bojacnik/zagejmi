using Zagejmi.Server.Domain.Auth;

namespace Zagejmi.Server.Domain.Events.Auth;

public sealed class UserUpdated : IDomainEvent<User, Guid>
{
    public Guid UserId { get; }
    public string Username { get; }
    public Password Password { get; }
    public string Email { get; }
    public DateTime Timestamp { get; }
    public EventTypeDomain EventType { get; }

    public UserUpdated(Guid userId, string username, Password password, string email)
    {
        UserId = userId;
        Username = username;
        Password = password;
        Email = email;
        Timestamp = DateTime.UtcNow;
        EventType = EventTypeDomain.UserUpdated;
    }

    public User Apply(User aggregate)
    {
        // The User aggregate's properties have private setters to enforce encapsulation.
        // State changes should be initiated via methods on the aggregate itself 
        // (e.g., user.ChangeEmail(newEmail)), which would then raise this event.
        // This event serves as a notification that an update occurred.
        return aggregate;
    }
}