using System;
using System.Collections.Generic;
using System.Linq;

namespace Zagejmi.Domain.Games;

/// <summary>
///     A game from the curated <see cref="GameCatalog" /> that services can be offered for.
/// </summary>
public sealed record Game
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Game" /> class.
    /// </summary>
    /// <param name="key">The stable key stored in the database (e.g. "valorant"). Never change it once released.</param>
    /// <param name="name">The display name.</param>
    /// <param name="rankNames">The rank tiers from lowest to highest; empty when the game has no ranks.</param>
    public Game(string key, string name, params string[] rankNames)
    {
        this.Key = key;
        this.Name = name;
        this.Ranks = rankNames
            .Select((rankName, index) => new GameRank(ToKey(rankName), rankName, index))
            .ToArray();
    }

    /// <summary>
    ///     Gets the stable key stored in the database.
    /// </summary>
    public string Key { get; }

    /// <summary>
    ///     Gets the display name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets the rank tiers from lowest to highest; empty when the game has no ranks.
    /// </summary>
    public IReadOnlyList<GameRank> Ranks { get; }

    /// <summary>
    ///     Gets a value indicating whether the game has ranks.
    /// </summary>
    public bool HasRanks => this.Ranks.Count > 0;

    /// <summary>
    ///     Finds a rank of this game by its key.
    /// </summary>
    /// <param name="rankKey">The rank key.</param>
    /// <returns>The rank, or <see langword="null" /> when the game has no such rank.</returns>
    public GameRank? FindRank(string? rankKey)
    {
        return rankKey is null ? null : this.Ranks.FirstOrDefault(r => r.Key == rankKey);
    }

    /// <summary>
    ///     Turns a display name into a key: lower case, words joined by dashes ("Silver Elite Master" → "silver-elite-master").
    /// </summary>
    private static string ToKey(string name)
    {
        return string.Join('-', name.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}

/// <summary>
///     A rank tier of a game.
/// </summary>
/// <param name="Key">The stable key stored in the database (e.g. "gold").</param>
/// <param name="Name">The display name.</param>
/// <param name="Order">The position from the lowest rank (0), for "this rank or higher" comparisons.</param>
public sealed record GameRank(string Key, string Name, int Order);
