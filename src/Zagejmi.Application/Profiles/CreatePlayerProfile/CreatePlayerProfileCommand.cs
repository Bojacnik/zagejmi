using System;

namespace Zagejmi.Application.Profiles.CreatePlayerProfile;

/// <summary>
///     Command to create the player profile (Hráč/Hráčka) of an existing user.
/// </summary>
public sealed class CreatePlayerProfileCommand
{
    /// <summary>
    ///     Gets the identifier of the user creating the profile.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    ///     Gets the public display name of the player profile.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    ///     Gets the optional profile description.
    /// </summary>
    public string? Description { get; init; }
}
