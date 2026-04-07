using System;

using Zagejmi.Write.Domain.Abstractions;

namespace Zagejmi.Write.Domain.Auth.Events;

/// <summary>
///     Represents a domain event that is raised when a new user is created in the system.
///     This event captures all relevant information about the user creation, including the user's ID,
///     authentication credentials (username, hashed password, and email), and metadata about when the event occurred.
/// </summary>
public sealed record UserCreatedEvent : IDomainEvent
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserCreatedEvent" /> class with the specified user information.
    /// </summary>
    /// <param name="aggregateId">The unique identifier of the user that was created.</param>
    /// <param name="username">The username of the newly created user.</param>
    /// <param name="passwordHash">The securely hashed password of the newly created user.</param>
    /// <param name="email">The email address of the newly created user.</param>
    public UserCreatedEvent(Guid aggregateId, string username, string passwordHash, string email)
    {
        this.AggregateId = aggregateId;
        this.Username = username;
        this.PasswordHash = passwordHash;
        this.Email = email;
        this.OccurredOn = DateTimeOffset.UtcNow;
    }

    /// <summary>
    ///     Gets the username of the newly created user.
    /// </summary>
    public string Username { get; }

    /// <summary>
    ///     Gets the securely hashed password of the newly created user.
    /// </summary>
    public string PasswordHash { get; }

    /// <summary>
    ///     Gets the email address of the newly created user.
    /// </summary>
    public string Email { get; }

    /// <summary>
    ///     Gets the unique identifier of the user that was created.
    /// </summary>
    public Guid AggregateId { get; }

    /// <summary>
    ///     Gets the UTC timestamp of when the user creation event occurred.
    /// </summary>
    public DateTimeOffset OccurredOn { get; }

    /// <summary>
    ///     Gets the optional version of the aggregate at the time of the event.
    ///     For user creation events, this is typically 0 (first version).
    /// </summary>
    public int? AggregateVersion => 0;
}