namespace Zagejmi.Client.Services;

public interface IAuthService
{
    Task<string?> LoginAsync(string username, string password);
}
