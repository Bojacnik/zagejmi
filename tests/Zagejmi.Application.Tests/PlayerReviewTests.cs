using System;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

using Zagejmi.Application.Profiles.Reviews.DeletePlayerReview;
using Zagejmi.Application.Profiles.Reviews.GetPlayerReviews;
using Zagejmi.Application.Profiles.Reviews.WritePlayerReview;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Orders;
using Zagejmi.Domain.Profiles.Customers;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Shared;

namespace Zagejmi.Application.Tests;

public sealed class PlayerReviewTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Customer_reviews_a_player_and_the_average_counts_it()
    {
        Setup s = new();
        (User jana, _) = s.Customer("jana");
        (User petr, _) = s.Customer("petr");

        Assert.True((await s.Write.Handle(new WritePlayerReviewCommand(jana.Id, s.Player.Id, 5, "  Super hra!  "), Ct)).IsSuccess);
        Assert.True((await s.Write.Handle(new WritePlayerReviewCommand(petr.Id, s.Player.Id, 4, "   "), Ct)).IsSuccess);

        PlayerReviewsView view = (await s.Get.Handle(new GetPlayerReviewsQuery(s.Player.Id, jana.Id), Ct)).Value;

        Assert.Equal(2, view.TotalCount);
        Assert.Equal(4.5, view.Summary.Average);
        Assert.Equal(2, view.Summary.Count);
        Assert.Equal(ReviewEligibility.CanReview, view.Eligibility);
        Assert.Equal("Super hra!", view.Mine?.Comment);
        Assert.Contains(view.Items, r => r is { Rating: 4, Comment: null });
    }

    [Fact]
    public async Task Writing_again_replaces_the_review_instead_of_adding_one()
    {
        Setup s = new();
        (User jana, CustomerProfile customer) = s.Customer("jana");

        await s.Write.Handle(new WritePlayerReviewCommand(jana.Id, s.Player.Id, 2, "Nic moc"), Ct);
        DateTimeOffset written = s.World.Clock.GetUtcNow();
        s.World.Clock.Advance(TimeSpan.FromDays(1));
        await s.Write.Handle(new WritePlayerReviewCommand(jana.Id, s.Player.Id, 5, "Napodruhé skvělé"), Ct);

        PlayerReview review = Assert.Single(s.World.Reviews.Items);
        Assert.Equal(customer.Id, review.AuthorProfileId);
        Assert.Equal(5, review.Rating);
        Assert.Equal("Napodruhé skvělé", review.Comment);
        Assert.Equal(written, review.CreatedAt);
        Assert.True(review.UpdatedAt > review.CreatedAt);
    }

    [Fact]
    public async Task Player_cannot_review_their_own_profile()
    {
        Setup s = new();

        Result result = await s.Write.Handle(new WritePlayerReviewCommand(s.Owner.Id, s.Player.Id, 5, null), Ct);
        PlayerReviewsView view = (await s.Get.Handle(new GetPlayerReviewsQuery(s.Player.Id, s.Owner.Id), Ct)).Value;

        Assert.Equal(ErrorType.Forbidden, result.Error?.Type);
        Assert.Empty(s.World.Reviews.Items);
        Assert.Equal(ReviewEligibility.OwnProfile, view.Eligibility);
    }

    [Fact]
    public async Task Ghost_reads_but_cannot_write_or_delete()
    {
        Setup s = new();
        (User jana, _) = s.Customer("jana");
        await s.Write.Handle(new WritePlayerReviewCommand(jana.Id, s.Player.Id, 4, "Před duchem"), Ct);
        jana.IsGhostMode = true;

        Result write = await s.Write.Handle(new WritePlayerReviewCommand(jana.Id, s.Player.Id, 1, null), Ct);
        Result delete = await s.Delete.Handle(new DeletePlayerReviewCommand(jana.Id, s.Player.Id), Ct);
        PlayerReviewsView view = (await s.Get.Handle(new GetPlayerReviewsQuery(s.Player.Id, jana.Id), Ct)).Value;

        Assert.Equal("User.GhostMode", write.Error?.Code);
        Assert.Equal("User.GhostMode", delete.Error?.Code);
        Assert.Equal(4, Assert.Single(s.World.Reviews.Items).Rating);
        Assert.Equal(ReviewEligibility.GhostMode, view.Eligibility);
        Assert.Equal(4, view.Mine?.Rating);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public async Task Rating_must_be_one_to_five_stars(int rating)
    {
        Setup s = new();
        (User jana, _) = s.Customer("jana");

        Result result = await s.Write.Handle(new WritePlayerReviewCommand(jana.Id, s.Player.Id, rating, null), Ct);

        Assert.Equal("Review.Rating", result.Error?.Code);
        Assert.Empty(s.World.Reviews.Items);
    }

    [Fact]
    public async Task Text_longer_than_the_limit_is_rejected()
    {
        Setup s = new();
        (User jana, _) = s.Customer("jana");

        Result result = await s.Write.Handle(
            new WritePlayerReviewCommand(jana.Id, s.Player.Id, 3, new string('a', PlayerReview.CommentMaxLength + 1)),
            Ct);

        Assert.Equal("Review.Comment", result.Error?.Code);
    }

    [Fact]
    public async Task Author_deletes_only_their_own_review()
    {
        Setup s = new();
        (User jana, _) = s.Customer("jana");
        (User petr, _) = s.Customer("petr");
        await s.Write.Handle(new WritePlayerReviewCommand(jana.Id, s.Player.Id, 5, null), Ct);

        Result petrDeletes = await s.Delete.Handle(new DeletePlayerReviewCommand(petr.Id, s.Player.Id), Ct);
        Assert.Equal(ErrorType.NotFound, petrDeletes.Error?.Type);
        Assert.Single(s.World.Reviews.Items);

        Result janaDeletes = await s.Delete.Handle(new DeletePlayerReviewCommand(jana.Id, s.Player.Id), Ct);
        Assert.True(janaDeletes.IsSuccess);
        Assert.Empty(s.World.Reviews.Items);
    }

    [Fact]
    public async Task Anonymous_visitor_sees_reviews_but_must_sign_in_to_write()
    {
        Setup s = new();
        (User jana, _) = s.Customer("jana");
        await s.Write.Handle(new WritePlayerReviewCommand(jana.Id, s.Player.Id, 3, null), Ct);

        PlayerReviewsView view = (await s.Get.Handle(new GetPlayerReviewsQuery(s.Player.Id, null), Ct)).Value;

        Assert.Equal(ReviewEligibility.SignedOut, view.Eligibility);
        Assert.Null(view.Mine);
        Assert.Equal(1, view.TotalCount);
    }

    [Fact]
    public async Task Unknown_player_is_not_found()
    {
        Setup s = new();
        (User jana, _) = s.Customer("jana");

        Result write = await s.Write.Handle(new WritePlayerReviewCommand(jana.Id, Guid.NewGuid(), 5, null), Ct);
        Result<PlayerReviewsView> read = await s.Get.Handle(new GetPlayerReviewsQuery(Guid.NewGuid(), jana.Id), Ct);

        Assert.Equal(ErrorType.NotFound, write.Error?.Type);
        Assert.Equal(ErrorType.NotFound, read.Error?.Type);
    }

    [Fact]
    public async Task Recommendation_needs_a_completed_order_where_the_service_was_provided()
    {
        Setup s = new();
        (User jana, CustomerProfile customer) = s.World.AddUser("jana", verified: true);

        Assert.Equal(ReviewEligibility.NoCompletedOrder, (await s.Get.Handle(new GetPlayerReviewsQuery(s.Player.Id, jana.Id), Ct)).Value.Eligibility);
        Assert.Equal("Review.NoCompletedOrder", (await s.Write.Handle(new WritePlayerReviewCommand(jana.Id, s.Player.Id, 5, null), Ct)).Error?.Code);

        // Neither a no-show nor an order still waiting for confirmation counts.
        s.World.AddOrder(customer.Id, s.Player.Id, OrderOutcome.CustomerNoShow);
        s.World.AddOrder(customer.Id, s.Player.Id, complete: false);
        Assert.Equal("Review.NoCompletedOrder", (await s.Write.Handle(new WritePlayerReviewCommand(jana.Id, s.Player.Id, 5, null), Ct)).Error?.Code);

        s.World.AddOrder(customer.Id, s.Player.Id);
        Assert.True((await s.Write.Handle(new WritePlayerReviewCommand(jana.Id, s.Player.Id, 5, null), Ct)).IsSuccess);
        Assert.Equal(ReviewEligibility.CanReview, (await s.Get.Handle(new GetPlayerReviewsQuery(s.Player.Id, jana.Id), Ct)).Value.Eligibility);
    }

    /// <summary>
    ///     A world with one player ("Owner" owns the player profile) and the three review handlers.
    /// </summary>
    private sealed class Setup
    {
        public Setup()
        {
            (this.Owner, _) = this.World.AddUser("hrac", verified: true);
            this.Player = new PlayerProfile(Guid.CreateVersion7(), this.Owner.Id, "Hráč");
            this.World.Profiles.Players.Add(this.Player);
            this.Write = new WritePlayerReviewHandler(
                this.World.Users, this.World.Profiles, this.World.Reviews, this.World.Orders, this.World.Clock);
            this.Delete = new DeletePlayerReviewHandler(this.World.Users, this.World.Profiles, this.World.Reviews);
            this.Get = new GetPlayerReviewsHandler(this.World.Users, this.World.Profiles, this.World.Reviews, this.World.Orders);
        }

        public TestWorld World { get; } = new();

        /// <summary>
        ///     Adds a customer who has completed one order with the player (what a recommendation requires).
        /// </summary>
        public (User User, CustomerProfile Customer) Customer(string name)
        {
            (User user, CustomerProfile customer) = this.World.AddUser(name, verified: true);
            this.World.AddOrder(customer.Id, this.Player.Id);
            return (user, customer);
        }

        public User Owner { get; }

        public PlayerProfile Player { get; }

        public WritePlayerReviewHandler Write { get; }

        public DeletePlayerReviewHandler Delete { get; }

        public GetPlayerReviewsHandler Get { get; }
    }
}
