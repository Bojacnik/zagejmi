using System.ComponentModel.DataAnnotations;
using SharedKernel;

namespace Zagejmi.Domain.Auth;

public sealed class User<TUserEvent> : Entity<TUserEvent> where TUserEvent : IDomainEvent
{
    #region Entity Properties

    public override ulong Id { get; }
    protected override ulong Version { get; set; }

    #endregion

    #region User Properties

    public string Username;
    public Password Password;

    [EmailAddress] public string Email;

    #endregion

    public User(ulong id, string username, Password password, string email)
    {
        Id = id;
        Username = username;
        Password = password;
        Email = email;
    }
}