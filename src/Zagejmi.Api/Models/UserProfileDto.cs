using System;
using Zagejmi.Domain.Profiles;

namespace Zagejmi.Api.Models;

/// <summary>
///     Profile projection returned for a user profile entry.
/// </summary>
public sealed class UserProfileDto
{
    /// <summary>
    ///     Gets or sets the profile aggregate ID.
    /// </summary>
    public Guid ProfileId { get; set; }

    /// <summary>
    ///     Gets or sets owning user ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    ///     Gets or sets profile type label (for example, "Personal").
    /// </summary>
    public string ProfileType { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets display name shown in UI.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets API-relative URL for profile image, when available.
    /// </summary>
    public string? ProfileImageUrl { get; set; }
}