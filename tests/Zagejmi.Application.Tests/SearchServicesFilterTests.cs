using Xunit;

using Zagejmi.Application.Profiles;
using Zagejmi.Application.Profiles.SearchServices;
using Zagejmi.Domain.Games;
using Zagejmi.Shared;

namespace Zagejmi.Application.Tests;

public sealed class SearchServicesFilterTests
{
    [Fact]
    public void No_filters_match_everything()
    {
        Result<ServiceSearchFilter> result = SearchServicesHandler.ToFilter(new SearchServicesQuery { Search = "  " });

        Assert.True(result.IsSuccess);
        Assert.Equal(new ServiceSearchFilter(null, null, null, PlayStyles.None), result.Value);
    }

    [Fact]
    public void Minimum_rank_expands_to_that_rank_and_all_higher()
    {
        Result<ServiceSearchFilter> result = SearchServicesHandler.ToFilter(
            new SearchServicesQuery { GameKey = "valorant", MinRankKey = "immortal", PlayStyles = PlayStyles.Competitive });

        Assert.True(result.IsSuccess);
        Assert.Equal("valorant", result.Value.GameKey);
        Assert.Equal(["immortal", "radiant"], result.Value.RankKeys!);
        Assert.Equal(PlayStyles.Competitive, result.Value.PlayStyles);
    }

    [Fact]
    public void Minimum_rank_without_a_game_is_ignored()
    {
        Result<ServiceSearchFilter> result = SearchServicesHandler.ToFilter(new SearchServicesQuery { MinRankKey = "gold" });

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.RankKeys);
    }

    [Fact]
    public void Gender_group_is_passed_on_and_unknown_groups_are_rejected()
    {
        Assert.Equal(
            GenderGroup.Women,
            SearchServicesHandler.ToFilter(new SearchServicesQuery { Gender = GenderGroup.Women }).Value.Gender);

        Result<ServiceSearchFilter> invalid = SearchServicesHandler.ToFilter(new SearchServicesQuery { Gender = (GenderGroup)7 });
        Assert.Equal("Services.Gender", invalid.Error!.Code);
    }

    [Theory]
    [InlineData("not-a-game", null, PlayStyles.None, "Services.Game")]
    [InlineData("valorant", "challenger", PlayStyles.None, "Services.Rank")]
    [InlineData(null, null, (PlayStyles)8, "Services.PlayStyles")]
    public void Rejects_unknown_values(string? game, string? rank, PlayStyles styles, string code)
    {
        Result<ServiceSearchFilter> result = SearchServicesHandler.ToFilter(
            new SearchServicesQuery { GameKey = game, MinRankKey = rank, PlayStyles = styles });

        Assert.True(result.IsFailure);
        Assert.Equal(code, result.Error!.Code);
    }
}
