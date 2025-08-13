using Zagejmi.Server.Domain.Auth;

namespace Zagejmi.Server.Domain.Events.Auth;

public sealed class UserDeleted : IDomainEvent<User, Guid>
{
    public Guid UserId { get; }
    public DateTime Timestamp { get; }
    public EventTypeDomain EventType { get; }

    public UserDeleted(Guid userId)
    {
        UserId = userId;
        Timestamp = DateTime.UtcNow;
        EventType = EventTypeDomain.UserDeleted;
    }

    public User Apply(User aggregate)
    {
        aggregate.MarkAsDeleted();
        return aggregate;
    }
}