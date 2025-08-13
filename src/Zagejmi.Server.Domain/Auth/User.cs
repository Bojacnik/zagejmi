using Zagejmi.Server.Domain.Entity;
using Zagejmi.Server.Domain.Events;

namespace Zagejmi.Server.Domain.Auth;

public sealed class User : AggregateRoot<User, Guid>
{
    #region User Properties

    public string Username { get; set; }
    public Password Password { get; set; }
    public string Email { get; set; }
    public bool Deleted { get; private set; }

    #endregion

    private readonly List<Guid> _personIds = [];
    public IReadOnlyList<Guid> PersonIds => _personIds.AsReadOnly();

    public User(Guid id, string username, Password password, string email) : base(id)
    {
        Username = username;
        Password = password;
        Email = email;
        Deleted = false;
    }

    // Private constructor for persistence frameworks
    private User(Guid id) : base(id) { }

    public void AddPerson(Guid personId)
    {
        if (_personIds.Contains(personId))
        {
            return;
        }
        _personIds.Add(personId);
    }

    public bool IsPasswordValid(Password password)
    {
        return Password.Equals(password);
    }
    
    public void MarkAsDeleted()
    {
        Deleted = true;
    }

    protected override void Apply(IDomainEvent<User, Guid> evt)
    {
        evt.Apply(this);
    }
}
