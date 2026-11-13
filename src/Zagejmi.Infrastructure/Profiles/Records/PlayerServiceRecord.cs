using System;

namespace Zagejmi.Infrastructure.Profiles.Records;

/// <summary>
///     Database row of a service offered by a player (table <c>profiles.PlayerServices</c>).
/// </summary>
public record PlayerServiceRecord
{
    /// <summary>
    ///     Gets the service ID.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     Gets the player profile that offers the service.
    /// </summary>
    public Guid PlayerProfileId { get; init; }

    /// <summary>
    ///     Gets or sets the service name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     Gets or sets what the service includes.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    ///     Gets or sets an optional category.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    ///     Gets or sets the price in Goins.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    ///     Gets or sets what the price is for, e.g. "/ hodina".
    /// </summary>
    public string? PriceUnit { get; set; }

    /// <summary>
    ///     Gets or sets an optional promo label.
    /// </summary>
    public string? PromoTag { get; set; }

    /// <summary>
    ///     Gets or sets the service image.
    /// </summary>
    public Guid? ImageId { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the service can be ordered right now.
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    ///     Gets when the service was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
}
