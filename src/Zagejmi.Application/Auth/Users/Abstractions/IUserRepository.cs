using System;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Domain.Auth;

namespace Zagejmi.Application.Auth.Users.Abstractions;

/// <summary>
///     Represents a repository for managing user entities.
/// </summary>
public interface IUserRepository : IBasicRepository<User, Guid>
{
    /// <summary>
    ///     Checks whether a user with the given user name or email already exists (case-insensitive).
    /// </summary>
    /// <param name="userName">The user name to check.</param>
    /// <param name="email">The email to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true" /> when either value is taken.</returns>
    Task<bool> IsUserNameOrEmailTakenAsync(
        string userName,
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds a user by user name or email (case-insensitive).
    /// </summary>
    /// <param name="userNameOrEmail">The user name or email.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The user, or <see langword="null" /> when none matches.</returns>
    Task<User?> GetByUserNameOrEmailAsync(string userNameOrEmail, CancellationToken cancellationToken = default);
}