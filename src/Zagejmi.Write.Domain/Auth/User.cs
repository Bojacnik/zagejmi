using System;
using System.Collections.Generic;

using Zagejmi.Write.Domain.Abstractions;
using Zagejmi.Write.Domain.Auth.Events;

namespace Zagejmi.Write.Domain.Auth;

/// <summary>
///     Represents a user in the system, encapsulating authentication and authorization information such as username,
///     password, email, and associated person IDs. The User aggregate is responsible for managing the state and behavior
///     related to user accounts, including creation, updates, and deletion. It ensures that all operations on the user
///     account are consistent and adhere to the business rules defined for user management.
/// </summary>
public sealed class User : Aggregate
{
    /// <summary>
    ///     A private list of person IDs associated with the user. This list is used to manage the relationships between the
    ///     user and the person entities in the domain model. The PersonIds property exposes a read-only view of this list to
    ///     ensure that external code cannot modify it directly, maintaining the integrity of the user aggregate. The user can
    ///     be associated with multiple person entities, and this list allows for efficient management of those associations
    ///     while keeping the internal state of the user aggregate consistent and encapsulated.
    /// </summary>
    private readonly List<Guid> _personIds = [];

    /// <summary>
    ///     Initializes a new instance of the <see cref="User" /> class with the specified unique identifier. This constructor
    ///     is private to enforce the use of factory methods for creating user instances, ensuring that all necessary
    ///     properties are properly initialized and that any business rules related to user creation are enforced. The unique
    ///     identifier (id) is essential for tracking and managing user accounts within the system, allowing for efficient
    ///     retrieval, updates, and associations with other entities in the domain model.
    /// </summary>
    /// <param name="id">
    ///     The unique identifier for the user, typically generated as a GUID to ensure uniqueness across
    ///     different instances and systems. This parameter is essential for tracking and managing user accounts effectively,
    ///     allowing for efficient retrieval, updates, and associations with other entities in the domain model.
    /// </param>
    private User(Guid id)
        : base(id)
    {
    }

    /// <summary>
    ///     Gets the authentication credentials associated with the user. The AuthCredentials property encapsulates the
    ///     necessary information for user authentication, including the username, securely hashed password, and email
    ///     address. This property is essential for managing user authentication and ensuring that the user's credentials are
    ///     securely stored and handled according to best practices. The AuthCredentials property should be treated as
    ///     sensitive information and should be protected to prevent unauthorized access, ensuring the security and integrity
    ///     of user accounts within the system.
    /// </summary>
    public AuthCredentials? AuthCredentials { get; private set; }

    /// <summary>
    ///     Gets a read-only list of person IDs associated with the user. The PersonIds property provides a way to manage the
    ///     relationships between the user and the person entities in the domain model. This property exposes a read-only view
    ///     of the internal list of person IDs, ensuring that external code cannot modify it directly, which helps maintain the
    ///     integrity of the user aggregate. The user can be associated with multiple person entities, and this list allows for
    ///     efficient management of those associations while keeping the internal state of the user aggregate consistent and
    ///     encapsulated. The PersonIds property is essential for tracking and managing the relationships between users and
    ///     person entities, allowing for efficient retrieval and updates of those associations within the system.
    /// </summary>
    public IReadOnlyList<Guid> PersonIds => this._personIds.AsReadOnly();

    /// <summary>
    ///     Factory method to create a new user with the specified authentication credentials.
    ///     This method creates a new User aggregate with the provided credentials and raises a UserCreatedEvent
    ///     to represent the user creation in the domain model.
    /// </summary>
    /// <param name="username">The username for the new user.</param>
    /// <param name="passwordHash">The securely hashed password for the new user.</param>
    /// <param name="email">The email address for the new user.</param>
    /// <returns>A new User aggregate instance with the specified credentials.</returns>
    public static User Create(string username, string passwordHash, string email)
    {
        Guid userId = Guid.CreateVersion7();
        User user = new(userId);
        user.RaiseEvent(new UserCreatedEvent(userId, username, passwordHash, email));
        return user;
    }

    /// <summary>
    ///     Applies the specified domain event to the user aggregate, updating its state based on the event's data. This method
    ///     is responsible for handling the logic of how the user aggregate should react to different types of domain events,
    ///     ensuring that the state of the user is consistent with the events that have occurred. The Apply method should be
    ///     implemented to handle all relevant domain events related to the user aggregate, such as user creation, updates to
    ///     authentication credentials, email changes, and associations with person entities. By applying domain events
    ///     correctly, the user aggregate can maintain its integrity and ensure that all operations on the user account adhere
    ///     to the business rules defined for user management.
    /// </summary>
    /// <param name="domainEvent">
    ///     The domain event to apply to the user aggregate. This parameter should be an instance of a class that implements
    ///     the IDomainEvent interface and contains the necessary data to update the state of the user aggregate. The Apply
    ///     method should handle different types of domain events related to the user aggregate, such as user creation, updates
    ///     to authentication credentials, email changes, and associations with person entities, ensuring that the state of the
    ///     user is consistent with the events that have occurred and that all operations on the user account adhere to the
    ///     business rules defined for user management.
    /// </param>
    protected override void Apply(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case UserCreatedEvent userCreatedEvent:
                this.ApplyUserCreated(userCreatedEvent);
                break;
            default:
                throw new InvalidOperationException($"Unknown event type: {domainEvent.GetType().Name}");
        }
    }

    /// <summary>
    ///     Applies a UserCreatedEvent to the aggregate state.
    /// </summary>
    private void ApplyUserCreated(UserCreatedEvent @event)
    {
        this.AuthCredentials = new AuthCredentials(
            @event.AggregateId,
            @event.Username,
            @event.PasswordHash,
            @event.Email);
    }
}