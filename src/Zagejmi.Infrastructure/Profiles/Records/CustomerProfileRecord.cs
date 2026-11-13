using System;

namespace Zagejmi.Infrastructure.Profiles.Records;

/// <summary>
///     Represents a customer profile record containing information about a customer.
/// </summary>
public record CustomerProfileRecord
{
    /// <summary>
    ///     Gets the unique identifier for the customer profile record.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     Gets or sets the identifier of the owning user (users live in a separate DbContext).
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    ///     Gets the time the profile was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    ///     Gets or sets the display name for the customer profile.
    /// </summary>
    public required string DisplayName { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the profile picture image blob.
    /// </summary>
    public Guid? ProfilePictureId { get; set; }
}