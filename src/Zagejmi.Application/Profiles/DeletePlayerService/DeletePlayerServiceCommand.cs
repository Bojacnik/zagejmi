using System;

namespace Zagejmi.Application.Profiles.DeletePlayerService;

/// <summary>
///     Represents a command to delete one of the user's player services.
/// </summary>
public sealed class DeletePlayerServiceCommand
{
    /// <summary>
    ///     Gets the user ID.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    ///     Gets the service to delete.
    /// </summary>
    public required Guid ServiceId { get; init; }
}
