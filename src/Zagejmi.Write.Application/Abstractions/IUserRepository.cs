using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Write.Domain.Auth;

namespace Zagejmi.Write.Application.Abstractions;

/// <summary>
///     Abstraction for accessing user data.
///     Implementations should retrieve users from the read model or reconstruct from events.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    ///     Gets a user by username.
    /// </summary>
    /// <param name="username">The username to search for.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The user if found, null otherwise.</returns>
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a user by email.
    /// </summary>
    /// <param name="email">The email to search for.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The user if found, null otherwise.</returns>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}