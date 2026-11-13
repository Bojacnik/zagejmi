using Xunit;

using Zagejmi.Application.Profiles;
using Zagejmi.Domain.Games;
using Zagejmi.Shared;

namespace Zagejmi.Application.Tests;

public sealed class PlayerServiceRulesTests
{
    [Theory]
    [InlineData(PlayStyles.Competitive)]
    [InlineData(PlayStyles.Casual)]
    [InlineData(PlayStyles.Both)]
    public void Accepts_a_game_with_rank_and_any_play_style(PlayStyles styles)
    {
        Result<ServiceFields> result = Validate("valorant", "gold", styles);

        Assert.True(result.IsSuccess);
        Assert.Equal(new GameFields("valorant", "gold", styles), result.Value.Game);
    }

    [Fact]
    public void Rank_is_optional()
    {
        Result<ServiceFields> result = Validate("minecraft", " ", PlayStyles.Casual);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Game.RankKey);
    }

    [Fact]
    public void Without_a_game_rank_and_play_style_are_cleared()
    {
        Result<ServiceFields> result = Validate(string.Empty, "gold", PlayStyles.Both);

        Assert.True(result.IsSuccess);
        Assert.Equal(new GameFields(null, null, PlayStyles.None), result.Value.Game);
    }

    [Theory]
    [InlineData("not-a-game", null, PlayStyles.Both, "Service.Game")]
    [InlineData("valorant", "challenger", PlayStyles.Both, "Service.Rank")]
    [InlineData("valorant", null, PlayStyles.None, "Service.PlayStyles")]
    [InlineData("valorant", null, (PlayStyles)4, "Service.PlayStyles")]
    public void Rejects_values_outside_the_catalog(string game, string? rank, PlayStyles styles, string code)
    {
        Result<ServiceFields> result = Validate(game, rank, styles);

        Assert.True(result.IsFailure);
        Assert.Equal(code, result.Error!.Code);
    }

    private static Result<ServiceFields> Validate(string? game, string? rank, PlayStyles styles)
    {
        return PlayerServiceRules.Validate("Duo", null, null, 100, null, game, rank, styles);
    }
}
