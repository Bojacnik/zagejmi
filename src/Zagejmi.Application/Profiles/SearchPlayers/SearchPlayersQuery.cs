namespace Zagejmi.Application.Profiles.SearchPlayers;

/// <summary>
///     Represents a query for the public player directory (Hráči page).
/// </summary>
public sealed class SearchPlayersQuery
{
    /// <summary>
    ///     The largest page size a caller may request.
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    ///     Gets the optional text to look for in the display name (case-insensitive).
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    ///     Gets the 1-based page number.
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    ///     Gets the number of players per page (at most <see cref="MaxPageSize" />).
    /// </summary>
    public int PageSize { get; init; } = 48;
}
