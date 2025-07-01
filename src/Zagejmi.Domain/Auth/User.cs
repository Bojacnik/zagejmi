using System.ComponentModel.DataAnnotations;
using SharedKernel;

namespace Zagejmi.Domain.Auth;

public sealed class User : Entity<ulong>
{
    #region User Properties

    public string Username;
    public Password Password;

    [EmailAddress] public string Email;

    #endregion

    public User(ulong id, string username, Password password, string email) : base(id)
    {
        Id = id;
        Username = username;
        Password = password;
        Email = email;
    }
}