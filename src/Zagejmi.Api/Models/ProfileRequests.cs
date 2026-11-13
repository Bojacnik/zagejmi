using System;
using System.Collections.Generic;
using Zagejmi.Domain.Profiles;

namespace Zagejmi.Api.Models;

/// <summary>
///     Request payload for creating a user profile.
/// </summary>
public sealed class UserCreateProfileCommand
{
    /// <summary>
    ///     Gets or sets the owning user identifier.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    ///     Gets or sets the profile type label.
    /// </summary>
    public string ProfileType { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the profile image as a data URL.
    /// </summary>
    public string? ProfileImageDataUrl { get; set; }
}

/// <summary>
///     Request payload for updating a profile image.
/// </summary>
public sealed class UserUpdateProfileImageCommand
{
    /// <summary>
    ///     Gets or sets the owning user identifier.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    ///     Gets or sets the profile identifier.
    /// </summary>
    public Guid ProfileId { get; set; }

    /// <summary>
    ///     Gets or sets the profile image as a data URL.
    /// </summary>
    public string ProfileImageDataUrl { get; set; } = string.Empty;
}

/// <summary>
///     Error payload returned by the API for validation failures.
/// </summary>
public sealed class ValidationErrorResponse
{
    /// <summary>
    ///     Gets the validation error messages.
    /// </summary>
    public List<string> Errors { get; init; } = [];
}
