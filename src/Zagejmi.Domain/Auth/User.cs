using System.ComponentModel.DataAnnotations;
using SharedKernel;
using Zagejmi.Domain.Community.People;

namespace Zagejmi.Domain.Auth;

public sealed record User : ValueObject
{
    #region User Properties

    public readonly string Username;
    public readonly Password Password;
    [EmailAddress] public readonly string Email;

    #endregion

    private readonly List<Person> _people = [];

    public User(string username, Password password, string email)
    {
        Username = username;
        Password = password;
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
        return Password.Equals(password);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Username;
        yield return Password;
        yield return Email;
    }
}