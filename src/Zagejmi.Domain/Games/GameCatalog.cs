using System.Collections.Generic;
using System.Linq;

namespace Zagejmi.Domain.Games;

/// <summary>
///     The curated list of games players can offer services for (MARKET-003). Players pick from this list instead of
///     typing the game, so the Služby filters can rely on it.
/// </summary>
/// <remarks>
///     Keys are stored in the database: add new games and ranks freely, but never rename or remove a key that is in use.
///     Ranks are tiers only (no divisions such as "Gold 2"), from lowest to highest.
/// </remarks>
public static class GameCatalog
{
    // A data table: key and name on the first line, the rank tiers wrapped below (one rank per line would be unreadable).
#pragma warning disable SA1117 // Parameters should be on same line or separate lines
    /// <summary>
    ///     Gets all games in display order.
    /// </summary>
    public static IReadOnlyList<Game> All { get; } =
    [
        new("league-of-legends", "League of Legends",
            "Iron", "Bronze", "Silver", "Gold", "Platinum", "Emerald", "Diamond", "Master", "Grandmaster", "Challenger"),
        new("valorant", "Valorant",
            "Iron", "Bronze", "Silver", "Gold", "Platinum", "Diamond", "Ascendant", "Immortal", "Radiant"),
        new("counter-strike-2", "Counter-Strike 2",
            "Silver I", "Silver II", "Silver III", "Silver IV", "Silver Elite", "Silver Elite Master",
            "Gold Nova I", "Gold Nova II", "Gold Nova III", "Gold Nova Master",
            "Master Guardian I", "Master Guardian II", "Master Guardian Elite", "Distinguished Master Guardian",
            "Legendary Eagle", "Legendary Eagle Master", "Supreme Master First Class", "Global Elite"),
        new("dota-2", "Dota 2",
            "Herald", "Guardian", "Crusader", "Archon", "Legend", "Ancient", "Divine", "Immortal"),
        new("overwatch-2", "Overwatch 2",
            "Bronze", "Silver", "Gold", "Platinum", "Diamond", "Master", "Grandmaster", "Champion", "Top 500"),
        new("marvel-rivals", "Marvel Rivals",
            "Bronze", "Silver", "Gold", "Platinum", "Diamond", "Grandmaster", "Celestial", "Eternity", "One Above All"),
        new("fortnite", "Fortnite",
            "Bronze", "Silver", "Gold", "Platinum", "Diamond", "Elite", "Champion", "Unreal"),
        new("apex-legends", "Apex Legends",
            "Rookie", "Bronze", "Silver", "Gold", "Platinum", "Diamond", "Master", "Apex Predator"),
        new("rocket-league", "Rocket League",
            "Bronze", "Silver", "Gold", "Platinum", "Diamond", "Champion", "Grand Champion", "Supersonic Legend"),
        new("rainbow-six-siege", "Rainbow Six Siege",
            "Copper", "Bronze", "Silver", "Gold", "Platinum", "Emerald", "Diamond", "Champion"),
        new("minecraft", "Minecraft"),
    ];
#pragma warning restore SA1117

    /// <summary>
    ///     Finds a game by its key.
    /// </summary>
    /// <param name="gameKey">The game key.</param>
    /// <returns>The game, or <see langword="null" /> when the key is not in the catalog.</returns>
    public static Game? Find(string? gameKey)
    {
        return gameKey is null ? null : All.FirstOrDefault(g => g.Key == gameKey);
    }
}
