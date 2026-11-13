using System;

namespace Zagejmi.Application.Auth.Users.SetGhostMode;

/// <summary>
///     Command to turn ghost mode on or off for a user.
/// </summary>
public sealed class SetGhostModeCommand
{
    /// <summary>
    ///     Gets the user identifier.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    ///     Gets a value indicating whether ghost mode should be enabled.
    /// </summary>
    public bool Enabled { get; init; }
}
