using System.ComponentModel.DataAnnotations;
using SharedKernel;
using Zagejmi.Domain.Community.People;

namespace Zagejmi.Domain.Auth;

public sealed record User : AggregateRoot<User, Guid>
{
    #region User Properties

    private string _username;
    private readonly Password _password;

    [EmailAddress] public string Email;

    #endregion

    private readonly List<Person> _people = [];

    public User(Guid id, string username, Password password, string email) : base(id)
    {
        Id = id;
        _username = username;
        _password = password;
        Email = email;
    }

    public void AddPerson(Person person)
    {
        if (_people.Contains(person))
        {
            return;
        }

        _people.Add(person);
    }

    public bool IsPasswordValid(Password password)
    {
        return _password.Equals(password);
    }

    protected override void Apply(IDomainEvent<User, Guid> evt)
    {
    }
}