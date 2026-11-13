using Zagejmi.Domain.Games;

namespace Zagejmi.Application.Profiles.SearchServices;

/// <summary>
///     Filters for the Služby page: all available services of all players.
/// </summary>
public sealed class SearchServicesQuery
{
    /// <summary>
    ///     The largest allowed page.
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    ///     Gets text to look for in the service name or the player's display name.
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    ///     Gets the game (a <see cref="GameCatalog" /> key), or <see langword="null" /> for any game.
    /// </summary>
    public string? GameKey { get; init; }

    /// <summary>
    ///     Gets the lowest rank to include (a rank key of <see cref="GameKey" />). Services without a rank are left out
    ///     when set. Ignored without a game.
    /// </summary>
    public string? MinRankKey { get; init; }

    /// <summary>
    ///     Gets the wanted play style: services offering any of the given styles match. <see cref="PlayStyles.None" />
    ///     for any service, including those without a game.
    /// </summary>
    public PlayStyles PlayStyles { get; init; }

    /// <summary>
    ///     Gets the gender group to list, or <see langword="null" /> for everyone (the combined list).
    /// </summary>
    public GenderGroup? Gender { get; init; }

    /// <summary>
    ///     Gets the 1-based page number.
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    ///     Gets how many services to return per page.
    /// </summary>
    public int PageSize { get; init; } = 48;
}
