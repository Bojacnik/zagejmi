using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging.Abstractions;

using Xunit;

using Zagejmi.Application.Wallets.ExpireBonuses;
using Zagejmi.Application.Wallets.GetWallet;
using Zagejmi.Application.Wallets.GetWalletHistory;
using Zagejmi.Application.Wallets.GrantReward;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Profiles;
using Zagejmi.Domain.Profiles.Customers;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Domain.Wallets;
using Zagejmi.Shared;

namespace Zagejmi.Application.Tests;

public sealed class WalletHandlerTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Wallet_shows_paid_and_bonus_parts_and_the_bonus_expiries()
    {
        TestWorld world = new();
        (User user, CustomerProfile customer) = world.AddUser("jana", verified: true);
        await Grant(world, customer, 30, "first");
        world.Clock.Advance(TimeSpan.FromDays(1));
        await Grant(world, customer, 20, "second");

        Result<WalletView> wallet = await new GetWalletHandler(world.Users, world.Profiles, world.Wallets, world.Clock)
            .Handle(new GetWalletQuery(user.Id, ProfileType.Customer), Ct);

        Assert.Equal(0, wallet.Value.PaidBalance);
        Assert.Equal(50, wallet.Value.BonusBalance);
        Assert.Equal(50, wallet.Value.Balance);
        Assert.Collection(
            wallet.Value.ActiveBonuses,
            b => Assert.Equal(30, b.Remaining),
            b => Assert.Equal(20, b.Remaining));
    }

    [Fact]
    public async Task Customer_and_player_wallets_are_separate()
    {
        TestWorld world = new();
        (User user, CustomerProfile customer) = world.AddUser("jana", verified: true);
        world.Profiles.Players.Add(new PlayerProfile(Guid.CreateVersion7(), user.Id, "Jana hraje"));
        await Grant(world, customer, 50, "first");

        GetWalletHandler wallets = new(world.Users, world.Profiles, world.Wallets, world.Clock);
        GetWalletHistoryHandler history = new(world.Users, world.Profiles, world.Wallets);

        Result<WalletView> player = await wallets.Handle(new GetWalletQuery(user.Id, ProfileType.Player), Ct);
        Result<WalletHistoryPage> playerHistory = await history.Handle(new GetWalletHistoryQuery(user.Id, ProfileType.Player), Ct);
        Result<WalletHistoryPage> customerHistory = await history.Handle(new GetWalletHistoryQuery(user.Id, ProfileType.Customer), Ct);

        Assert.Equal(0, player.Value.Balance);
        Assert.Empty(playerHistory.Value.Items);
        Assert.Single(customerHistory.Value.Items);
    }

    [Fact]
    public async Task Player_wallet_needs_a_player_profile()
    {
        TestWorld world = new();
        (User user, _) = world.AddUser("jana", verified: true);

        Result<WalletView> result = await new GetWalletHandler(world.Users, world.Profiles, world.Wallets, world.Clock)
            .Handle(new GetWalletQuery(user.Id, ProfileType.Player), Ct);

        Assert.Equal("Player.Missing", result.Error!.Code);
    }

    [Fact]
    public async Task History_pages_report_whether_more_exist()
    {
        TestWorld world = new();
        (User user, CustomerProfile customer) = world.AddUser("jana", verified: true);
        for (int i = 0; i < 3; i++)
        {
            await Grant(world, customer, 10, $"reward-{i}");
            world.Clock.Advance(TimeSpan.FromMinutes(1));
        }

        GetWalletHistoryHandler handler = new(world.Users, world.Profiles, world.Wallets);
        WalletHistoryPage first = (await handler.Handle(new GetWalletHistoryQuery(user.Id, ProfileType.Customer, 0, 2), Ct)).Value;
        WalletHistoryPage second = (await handler.Handle(new GetWalletHistoryQuery(user.Id, ProfileType.Customer, 2, 2), Ct)).Value;

        Assert.Equal(2, first.Items.Count);
        Assert.True(first.HasMore);
        Assert.Single(second.Items);
        Assert.False(second.HasMore);
    }

    [Fact]
    public async Task Expired_bonuses_go_back_to_the_promo_budget()
    {
        TestWorld world = new();
        (_, CustomerProfile customer) = world.AddUser("jana", verified: true);
        await Grant(world, customer, 30, "old");
        world.Clock.Advance(TimeSpan.FromDays(30));
        await Grant(world, customer, 20, "newer");

        // Only the first grant is past its 90 days.
        world.Clock.Advance(TimeSpan.FromDays(61));
        ExpireBonusesHandler handler = new(world.Wallets, world.Clock, NullLogger<ExpireBonusesHandler>.Instance);

        Assert.Equal(1, await handler.Handle(Ct));
        Assert.Equal(0, await handler.Handle(Ct));

        Account bonus = Assert.Single(world.Wallets.Accounts, a => a.OwnerProfileId == customer.Id);
        Assert.Equal(20, bonus.Balance);
        Assert.Equal(-20, Assert.Single(world.Wallets.Accounts, a => a.Kind == AccountKind.PromoBudget).Balance);
    }

    [Fact]
    public async Task Reward_with_the_same_key_is_granted_only_once()
    {
        TestWorld world = new();
        (_, CustomerProfile customer) = world.AddUser("jana", verified: true);

        Result<bool> first = await Grant(world, customer, 50, "same");
        Result<bool> second = await Grant(world, customer, 50, "same");

        Assert.True(first.Value);
        Assert.False(second.Value);
        Assert.Equal(50, Assert.Single(world.Wallets.Accounts, a => a.OwnerProfileId == customer.Id).Balance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Reward_must_be_positive(long amount)
    {
        TestWorld world = new();
        (_, CustomerProfile customer) = world.AddUser("jana", verified: true);

        Result<bool> result = await Grant(world, customer, amount, "k");

        Assert.Equal("Wallet.RewardAmount", result.Error!.Code);
    }

    private static Task<Result<bool>> Grant(TestWorld world, CustomerProfile customer, long amount, string key)
    {
        return world.GrantReward.Handle(
            new GrantRewardCommand
            {
                CustomerProfileId = customer.Id,
                Amount = amount,
                Reason = "Odměna",
                IdempotencyKey = key,
            },
            Ct);
    }
}
