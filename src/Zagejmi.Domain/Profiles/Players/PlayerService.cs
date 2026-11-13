using System;

namespace Zagejmi.Domain.Profiles.Players;

/// <summary>
///     A service a player offers (e.g. "Společné hraní Valorantu, 1 hodina").
/// </summary>
public record PlayerService
{
    /// <summary>
    ///     The longest allowed name.
    /// </summary>
    public const int NameMaxLength = 100;

    /// <summary>
    ///     The longest allowed description.
    /// </summary>
    public const int DescriptionMaxLength = 1000;

    /// <summary>
    ///     The longest allowed category.
    /// </summary>
    public const int CategoryMaxLength = 50;

    /// <summary>
    ///     The longest allowed price unit.
    /// </summary>
    public const int PriceUnitMaxLength = 20;

    /// <summary>
    ///     Gets the service ID.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    ///     Gets the player profile that offers the service.
    /// </summary>
    public required Guid PlayerProfileId { get; init; }

    /// <summary>
    ///     Gets or sets the service name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets what the service includes.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the price in Goins.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    ///     Gets or sets what the price is for, e.g. "/ hodina". Optional.
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
    ///     Gets or sets an optional category (e.g. a game).
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    ///     Gets or sets the average rating.
    /// </summary>
    public decimal Rating { get; set; }

    /// <summary>
    ///     Gets or sets the reviews of the service.
    /// </summary>
    public ServiceReview[] Reviews { get; set; } = [];

    /// <summary>
    ///     Gets when the service was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
