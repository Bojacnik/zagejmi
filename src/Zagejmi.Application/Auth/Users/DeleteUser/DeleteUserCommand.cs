using System;

namespace Zagejmi.Application.Auth.Users.DeleteUser;

/// <summary>
///     Command to delete a user account.
/// </summary>
public sealed class DeleteUserCommand
{
    /// <summary>
    ///     Gets the identifier of the user to delete.
    /// </summary>
    public Guid Id { get; init; }
}
