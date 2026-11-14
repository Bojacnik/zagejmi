using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Xunit;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Orders;
using Zagejmi.Application.Orders.GetOrders;
using Zagejmi.Application.Profiles.GetPlayerDetail;
using Zagejmi.Application.Profiles.Reviews;
using Zagejmi.Application.Profiles.Reviews.DeleteServiceReview;
using Zagejmi.Application.Profiles.Reviews.GetServiceReviews;
using Zagejmi.Application.Profiles.Reviews.WriteServiceReview;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Orders;
using Zagejmi.Domain.Profiles.Customers;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Shared;

namespace Zagejmi.Application.Tests;

public sealed class ServiceReviewTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Every_completed_order_can_be_reviewed_once_so_regulars_review_often()
    {
        Setup s = new();
        Order first = s.World.AddOrder(s.Customer.Id, s.Player.Id, serviceId: s.ServiceId);
        Order second = s.World.AddOrder(s.Customer.Id, s.Player.Id, serviceId: s.ServiceId);

        Assert.True((await s.Write(first.Id, 4, "Dobrý koučink")).IsSuccess);
        Assert.True((await s.Write(second.Id, 5, null)).IsSuccess);

        Assert.Equal(2, s.World.ServiceReviews.Items.Count);
        ServiceReview review = s.World.ServiceReviews.Items.Single(r => r.OrderId == first.Id);
        Assert.Equal("Valorant duo", review.ServiceName);
        Assert.Equal(s.Player.Id, review.PlayerProfileId);
        Assert.Equal(s.Customer.Id, review.AuthorProfileId);
        Assert.Equal(2, (await s.List()).TotalCount);
    }

    [Fact]
    public async Task Writing_again_edits_the_review_of_that_order()
    {
        Setup s = new();
        Order order = s.World.AddOrder(s.Customer.Id, s.Player.Id);
        await s.Write(order.Id, 2, "Nic moc");
        s.World.Clock.Advance(TimeSpan.FromHours(1));

        await s.Write(order.Id, 4, "Nakonec dobrý");

        ServiceReview review = Assert.Single(s.World.ServiceReviews.Items);
        Assert.Equal(4, review.Rating);
        Assert.True(review.UpdatedAt > review.CreatedAt);
        Assert.Equal(4, (await s.Get(order.Id)).Value?.Rating);
    }

    [Fact]
    public async Task Only_completed_delivered_orders_of_the_customer_can_be_reviewed_and_not_by_ghosts()
    {
        Setup s = new();
        Order noShow = s.World.AddOrder(s.Customer.Id, s.Player.Id, OrderOutcome.CustomerNoShow);
        Order waiting = s.World.AddOrder(s.Customer.Id, s.Player.Id, complete: false);
        (User other, CustomerProfile otherCustomer) = s.World.AddUser("petr", verified: true);
        Order someoneElses = s.World.AddOrder(otherCustomer.Id, s.Player.Id);

        Assert.Equal("Review.OrderNotCompleted", (await s.Write(noShow.Id, 5, null)).Error?.Code);
        Assert.Equal("Review.OrderNotCompleted", (await s.Write(waiting.Id, 5, null)).Error?.Code);
        Assert.Equal(ErrorType.NotFound, (await s.Write(someoneElses.Id, 5, null)).Error?.Type);
        Assert.Equal(ErrorType.NotFound, (await s.Write(Guid.NewGuid(), 5, null)).Error?.Type);
        Assert.Equal("Review.Rating", (await s.Write(someoneElses.Id, 6, null)).Error?.Code);

        Order done = s.World.AddOrder(s.Customer.Id, s.Player.Id);
        s.User.IsGhostMode = true;
        Assert.Equal("User.GhostMode", (await s.Write(done.Id, 5, null)).Error?.Code);
        Assert.Empty(s.World.ServiceReviews.Items);
        _ = other;
    }

    [Fact]
    public async Task Author_can_delete_the_review()
    {
        Setup s = new();
        Order order = s.World.AddOrder(s.Customer.Id, s.Player.Id);
        await s.Write(order.Id, 3, null);

        Assert.True((await s.DeleteHandler.Handle(new DeleteServiceReviewCommand(s.User.Id, order.Id), Ct)).IsSuccess);
        Assert.Empty(s.World.ServiceReviews.Items);
        Assert.Equal(ErrorType.NotFound, (await s.DeleteHandler.Handle(new DeleteServiceReviewCommand(s.User.Id, order.Id), Ct)).Error?.Type);
    }

    [Fact]
    public async Task Order_list_tells_which_completed_orders_are_already_reviewed()
    {
        Setup s = new();
        Order reviewed = s.World.AddOrder(s.Customer.Id, s.Player.Id);
        Order notYet = s.World.AddOrder(s.Customer.Id, s.Player.Id);
        await s.Write(reviewed.Id, 5, null);

        GetOrdersHandler handler = new(s.World.Profiles, s.World.Orders, s.World.ServiceReviews, s.World.Clock);
        PagedResult<OrderView> orders = (await handler.Handle(new GetOrdersQuery(s.User.Id, OrderRole.Customer), Ct)).Value;

        Assert.True(orders.Items.Single(v => v.Order.Id == reviewed.Id).HasReview);
        Assert.False(orders.Items.Single(v => v.Order.Id == notYet.Id).HasReview);
    }

    [Fact]
    public async Task Service_average_shows_only_from_five_reviews()
    {
        Setup s = new();
        s.World.Profiles.Services.Add(new PlayerService { Id = s.ServiceId, PlayerProfileId = s.Player.Id, Name = "Valorant duo" });
        GetPlayerDetailHandler detail = new(
            s.World.Profiles, s.World.Users, s.World.ServiceReviews, Options.Create(new ReviewOptions()));

        for (int i = 0; i < 4; i++)
        {
            await s.Write(s.World.AddOrder(s.Customer.Id, s.Player.Id, serviceId: s.ServiceId).Id, 4, null);
        }

        Assert.Null(Assert.Single((await detail.Handle(s.Player.Id, Ct)).Value.Services).Rating);

        await s.Write(s.World.AddOrder(s.Customer.Id, s.Player.Id, serviceId: s.ServiceId).Id, 5, null);
        Assert.Equal(new RatingSummary(4.2, 5), Assert.Single((await detail.Handle(s.Player.Id, Ct)).Value.Services).Rating);
    }

    [Theory]
    [InlineData(4, false)]
    [InlineData(5, true)]
    [InlineData(12, true)]
    public void Review_threshold_is_configurable_and_defaults_to_five(int count, bool visible)
    {
        Assert.Equal(visible, new ReviewOptions().Visible(new RatingSummary(4, count)) is not null);
        Assert.NotNull(new ReviewOptions { MinReviewsToShowRating = 1 }.Visible(new RatingSummary(4, 1)));
        Assert.Null(new ReviewOptions().Visible(null));
    }

    /// <summary>
    ///     A customer "jana" and a player with one service, plus the service review handlers.
    /// </summary>
    private sealed class Setup
    {
        public Setup()
        {
            (this.User, this.Customer) = this.World.AddUser("jana", verified: true);
            (User owner, _) = this.World.AddUser("eva", verified: true);
            this.Player = new PlayerProfile(Guid.CreateVersion7(), owner.Id, "Hráčka Eva");
            this.World.Profiles.Players.Add(this.Player);
            this.WriteHandler = new WriteServiceReviewHandler(
                this.World.Users, this.World.Profiles, this.World.Orders, this.World.ServiceReviews, this.World.Clock);
            this.DeleteHandler = new DeleteServiceReviewHandler(
                this.World.Users, this.World.Profiles, this.World.Orders, this.World.ServiceReviews);
            this.GetHandler = new GetOrderReviewHandler(
                this.World.Users, this.World.Profiles, this.World.Orders, this.World.ServiceReviews);
        }

        public TestWorld World { get; } = new();

        public User User { get; }

        public CustomerProfile Customer { get; }

        public PlayerProfile Player { get; }

        public Guid ServiceId { get; } = Guid.CreateVersion7();

        public WriteServiceReviewHandler WriteHandler { get; }

        public DeleteServiceReviewHandler DeleteHandler { get; }

        public GetOrderReviewHandler GetHandler { get; }

        public Task<Result> Write(Guid orderId, int rating, string? comment) =>
            this.WriteHandler.Handle(new WriteServiceReviewCommand(this.User.Id, orderId, rating, comment), Ct);

        public Task<Result<ServiceReview?>> Get(Guid orderId) =>
            this.GetHandler.Handle(new GetOrderReviewQuery(this.User.Id, orderId), Ct);

        public Task<PagedResult<ServiceReviewItem>> List() =>
            new GetServiceReviewsHandler(this.World.ServiceReviews).Handle(new GetServiceReviewsQuery(this.Player.Id), Ct);
    }
}
