using System;

using Zagejmi.Domain.Profiles;

namespace Zagejmi.Application.Auth.Users.SetDefaultProfile;

/// <summary>
///     Represents a command to choose which profile is active right after the user signs in.
/// </summary>
public sealed class SetDefaultProfileCommand
{
    /// <summary>
    ///     Gets the user ID.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    ///     Gets the profile type to use by default.
    /// </summary>
    public ProfileType Profile { get; init; }
}
