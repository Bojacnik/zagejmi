using System.ComponentModel.DataAnnotations;

namespace Zagejmi.Domain.Auth;

public class User
{
    public string Username;
    public Password Password;

    [EmailAddress] public string Email;

    public User(string username, Password password, string email)
    {
        this.Username = username;
        this.Password = password;
        this.Email = email;
    }
}