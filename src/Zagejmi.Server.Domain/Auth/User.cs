using LanguageExt;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using Zagejmi.Server.Domain.Entity;
using Zagejmi.Server.Domain.Events;
using Zagejmi.Server.Domain.Events.Auth;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Server.Domain.Auth;

public sealed class User : Aggregate<User, Guid>
{
    #region User Properties

    public string Username { get; private set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;
    public string Salt { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public bool Deleted { get; private set; }

    #endregion

    private readonly List<Guid> _personIds = [];
    public IReadOnlyList<Guid> PersonIds => _personIds.AsReadOnly();

    // Private constructor for rehydrating the aggregate from the event store.
    private User(Guid id) : base(id) { }

    public static Either<Failure, User> Create(Guid id, string username, string passwordHash, string salt, string email)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return new FailureArgumentInvalidValue("Username cannot be empty.");
        }

        if (!MailAddress.TryCreate(email, out _))
        {
            return new FailureArgumentInvalidValue("Invalid email address format.");
        }

        User user = new(id);
        user.RaiseEvent(new EventUserCreated(id, username, passwordHash, salt, email));
        return user;
    }

    #region Behavior Methods

    public void ChangeEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail) || Email == newEmail) return;

        RaiseEvent(new EventUserEmailChanged(Id, newEmail));
    }

    public bool IsPasswordValid(string password, IHashHandler hashHandler)
    {
        return hashHandler.Verify(password, Password, Salt);
    }

    public void MarkAsDeleted()
    {
        // Logic to raise a UserDeleted event would go here.
        Deleted = true;
    }

    #endregion

    // The Apply method is the core of the aggregate's state management.
    protected override void Apply(IDomainEvent<User, Guid> evt)
    {
        switch (evt)
        {
            case EventUserCreated e: OnUserCreated(e); break;
            case EventUserEmailChanged e: OnEmailChanged(e); break;
        }
    }

    #region State Mutators

    private void OnUserCreated(EventUserCreated evt)
    {
        Username = evt.Username;
        Password = evt.Password;
        Salt = evt.Salt;
        Email = evt.Email;
        Deleted = false;
    }

    private void OnEmailChanged(EventUserEmailChanged evt)
    {
        Email = evt.NewEmail;
    }

    #endregion
}
