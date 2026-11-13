using System;
using System.Collections.Generic;

using Zagejmi.Domain.Auth;

namespace Zagejmi.Api.Models;

/// <summary>
///     Public view of a player profile (Hráč/Hráčka), including the services it offers.
/// </summary>
public sealed class PlayerProfileDto
{
    /// <summary>
    ///     Gets or sets the profile identifier.
    /// </summary>
    public Guid ProfileId { get; set; }

    /// <summary>
    ///     Gets or sets the profile type label.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the player's grammatical gender, used to show "Hráč" or "Hráčka".
    /// </summary>
    public GrammaticalGender GrammaticalGender { get; set; }

    /// <summary>
    ///     Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the biography text.
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    ///     Gets or sets the profile image URL.
    /// </summary>
    public string? ProfileImageUrl { get; set; }

    /// <summary>
    ///     Gets or sets the banner image URL.
    /// </summary>
    public string? BannerImageUrl { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the player is online.
    /// </summary>
    public bool IsOnline { get; set; }

    /// <summary>
    ///     Gets or sets the reputation score.
    /// </summary>
    public double Reputation { get; set; }

    /// <summary>
    ///     Gets or sets the profile level.
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    ///     Gets or sets the number of completed sessions.
    /// </summary>
    public int CompletedSessions { get; set; }

    /// <summary>
    ///     Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    ///     Gets or sets the services offered by the player.
    /// </summary>
    public List<ServiceDto> Services { get; set; } = [];
}

/// <summary>
///     Public view of a service offered by a player.
/// </summary>
public sealed class ServiceDto
{
    /// <summary>
    ///     Gets or sets the service identifier.
    /// </summary>
    public Guid ServiceId { get; set; }

    /// <summary>
    ///     Gets or sets the service name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the service description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the service category.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    ///     Gets or sets the service price.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    ///     Gets or sets the price suffix (for example "/ hour").
    /// </summary>
    public string? PriceSuffix { get; set; }

    /// <summary>
    ///     Gets or sets the service image URL.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    ///     Gets or sets an optional promotional tag.
    /// </summary>
    public string? PromoTag { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the service can currently be booked.
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    ///     Gets or sets the average rating.
    /// </summary>
    public double Rating { get; set; }

    /// <summary>
    ///     Gets or sets how many times the service was delivered.
    /// </summary>
    public int Served { get; set; }

    /// <summary>
    ///     Gets or sets the average response time label.
    /// </summary>
    public string? ResponseTime { get; set; }
}
