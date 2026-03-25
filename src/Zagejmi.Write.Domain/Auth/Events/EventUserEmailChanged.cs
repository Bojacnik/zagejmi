namespace Zagejmi.Write.Domain.Auth.Events;

public sealed record EventUserEmailChanged : IDomainEvent<User, Guid>
{
    public EventUserEmailChanged(Guid userId, string newEmail)
    {
        this.UserId = userId;
        this.NewEmail = newEmail;
        this.Timestamp = DateTime.UtcNow;
        this.EventType = EventTypeDomain.UserEmailChanged;
    }

    public Guid UserId { get; }

    public string NewEmail { get; }

    public DateTime Timestamp { get; }

    public EventTypeDomain EventType { get; }

    public Guid AggregateId => this.UserId;
}