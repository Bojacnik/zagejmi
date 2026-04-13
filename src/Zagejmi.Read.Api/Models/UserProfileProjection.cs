using System;

namespace Zagejmi.Read.Api.Models;

/// <summary>
///     Read-side projection for a user profile aggregate.
/// </summary>
public sealed class UserProfileProjection
{
    /// <summary>
    ///     Gets or sets the profile identifier.
    /// </summary>
    public Guid ProfileId { get; set; }

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
    ///     Gets or sets the blob key for the profile image.
    /// </summary>
    public string? ProfileImageBlobKey { get; set; }

    /// <summary>
    ///     Gets or sets last update timestamp in UTC.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }
}