using System.Threading.Tasks;

namespace Zagejmi.Api.Components;

/// <summary>
///     Abstraction for UI authentication operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    ///     Attempts to authenticate a user and returns an access token.
    /// </summary>
    /// <param name="username">User name.</param>
    /// <param name="password">Plain text password.</param>
    /// <returns>JWT token if credentials are valid; otherwise <see langword="null" />.</returns>
    Task<string?> LoginAsync(string username, string password);
}