using System.Linq;

using Xunit;

using Zagejmi.Domain.Games;

namespace Zagejmi.Domain.Tests.Games;

public sealed class GameCatalogTests
{
    [Fact]
    public void Game_keys_are_unique()
    {
        Assert.Equal(GameCatalog.All.Count, GameCatalog.All.Select(g => g.Key).Distinct().Count());
    }

    [Fact]
    public void Rank_keys_are_unique_and_ordered_within_each_game()
    {
        foreach (Game game in GameCatalog.All)
        {
            Assert.Equal(game.Ranks.Count, game.Ranks.Select(r => r.Key).Distinct().Count());
            Assert.Equal(Enumerable.Range(0, game.Ranks.Count), game.Ranks.Select(r => r.Order));
        }
    }

    [Fact]
    public void Keys_fit_the_database_column()
    {
        Assert.All(GameCatalog.All, g => Assert.InRange(g.Key.Length, 1, Profiles.Players.PlayerService.GameKeyMaxLength));
        Assert.All(
            GameCatalog.All.SelectMany(g => g.Ranks),
            r => Assert.InRange(r.Key.Length, 1, Profiles.Players.PlayerService.GameKeyMaxLength));
    }

    [Fact]
    public void Finds_games_and_ranks_by_key()
    {
        Game? cs = GameCatalog.Find("counter-strike-2");

        Assert.NotNull(cs);
        Assert.Equal("Silver Elite Master", cs.FindRank("silver-elite-master")?.Name);
        Assert.Null(cs.FindRank("radiant"));
        Assert.Null(GameCatalog.Find("unknown-game"));
        Assert.False(GameCatalog.Find("minecraft")!.HasRanks);
    }
}
