using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

using Zagejmi.Application.Auth.Users.CreateUser;
using Zagejmi.Application.Auth.Users.EmailVerification;
using Zagejmi.Application.Auth.Users.LoginUser;
using Zagejmi.Application.Auth.Users.ResendVerificationEmail;
using Zagejmi.Application.Auth.Users.VerifyEmail;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Profiles.Customers;
using Zagejmi.Domain.Wallets;
using Zagejmi.Shared;

namespace Zagejmi.Application.Tests;

public sealed class EmailVerificationTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Registration_stores_only_a_token_hash_and_emails_the_link()
    {
        TestWorld world = new();
        CreateUserHandler handler = new(world.Users, new FakePasswordHasher(), world.Profiles, world.Mailer);

        Result<Guid> result = await handler.Handle(
            new CreateUserCommand { UserName = "jana", Email = "jana@example.test", Password = "heslo123", Gender = Gender.Female },
            Ct);

        Assert.True(result.IsSuccess);
        User user = Assert.Single(world.Users.Items);
        Assert.Null(user.AuthCredentials.EmailVerifiedAt);
        Assert.Equal($"hash:{world.Tokens.LastToken}", user.AuthCredentials.PendingEmailVerificationTokenHash);
        Assert.Equal(world.Clock.GetUtcNow() + EmailVerificationMailer.TokenLifetime, user.AuthCredentials.PendingEmailVerificationExpiresAt);

        (string to, _, string body) = Assert.Single(world.Email.Sent);
        Assert.Equal("jana@example.test", to);
        Assert.Contains($"token={world.Tokens.LastToken}", body, StringComparison.Ordinal);
        Assert.Contains("50 Goinů", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Registration_succeeds_even_when_the_email_cannot_be_sent()
    {
        TestWorld world = new();
        world.Email.Fail = true;
        CreateUserHandler handler = new(world.Users, new FakePasswordHasher(), world.Profiles, world.Mailer);

        Result<Guid> result = await handler.Handle(
            new CreateUserCommand { UserName = "jana", Email = "jana@example.test", Password = "heslo123", Gender = Gender.Female },
            Ct);

        Assert.True(result.IsSuccess);
        Assert.Single(world.Profiles.Customers);
    }

    [Fact]
    public async Task Valid_link_verifies_and_grants_the_sign_up_bonus_once()
    {
        TestWorld world = new();
        (User user, CustomerProfile customer) = world.AddUser("jana", verified: false);
        string token = world.Mailer.IssueToken(user);
        VerifyEmailHandler handler = NewVerifyHandler(world);

        Result<long> first = await handler.Handle(new VerifyEmailCommand(user.Id, token), Ct);
        Result<long> again = await handler.Handle(new VerifyEmailCommand(user.Id, token), Ct);

        Assert.Equal(50, first.Value);
        Assert.Equal(50, again.Value);
        Assert.Equal(world.Clock.GetUtcNow(), user.AuthCredentials.EmailVerifiedAt);
        Assert.Null(user.AuthCredentials.PendingEmailVerificationTokenHash);

        LedgerTransaction reward = Assert.Single(world.Wallets.Posted);
        Assert.Equal($"signup-bonus:{user.Id}", reward.IdempotencyKey);
        Assert.Equal(VerifyEmailHandler.SignUpBonusReason, reward.Description);

        Account bonus = Assert.Single(world.Wallets.Accounts, a => a.OwnerProfileId == customer.Id);
        Assert.Equal(AccountKind.CustomerBonus, bonus.Kind);
        Assert.Equal(50, bonus.Balance);
        BonusGrant grant = Assert.Single(world.Wallets.Grants);
        Assert.Equal(world.Clock.GetUtcNow().AddDays(90), grant.ExpiresAt);
    }

    [Fact]
    public async Task Wrong_token_does_not_verify()
    {
        TestWorld world = new();
        (User user, _) = world.AddUser("jana", verified: false);
        world.Mailer.IssueToken(user);

        Result<long> result = await NewVerifyHandler(world).Handle(new VerifyEmailCommand(user.Id, "guess"), Ct);

        Assert.Equal("User.VerificationLinkInvalid", result.Error!.Code);
        Assert.Null(user.AuthCredentials.EmailVerifiedAt);
        Assert.Empty(world.Wallets.Posted);
    }

    [Fact]
    public async Task Expired_token_does_not_verify()
    {
        TestWorld world = new();
        (User user, _) = world.AddUser("jana", verified: false);
        string token = world.Mailer.IssueToken(user);
        world.Clock.Advance(EmailVerificationMailer.TokenLifetime);

        Result<long> result = await NewVerifyHandler(world).Handle(new VerifyEmailCommand(user.Id, token), Ct);

        Assert.True(result.IsFailure);
        Assert.Null(user.AuthCredentials.EmailVerifiedAt);
    }

    [Fact]
    public async Task No_bonus_when_it_is_turned_off()
    {
        TestWorld world = new(signUpBonus: 0);
        (User user, _) = world.AddUser("jana", verified: false);
        string token = world.Mailer.IssueToken(user);

        Result<long> result = await NewVerifyHandler(world).Handle(new VerifyEmailCommand(user.Id, token), Ct);

        Assert.Equal(0, result.Value);
        Assert.NotNull(user.AuthCredentials.EmailVerifiedAt);
        Assert.Empty(world.Wallets.Posted);
    }

    [Fact]
    public async Task Login_requires_a_verified_email_but_only_reveals_it_after_a_correct_password()
    {
        TestWorld world = new();
        world.AddUser("jana", verified: false);
        FakeAccessTokens tokens = new();
        LoginUserHandler handler = new(world.Users, new FakePasswordHasher(), tokens, tokens);

        Result<LoginUserResult> wrongPassword = await handler.Handle(
            new LoginUserCommand { UserIdentifier = "jana", Password = "spatne" },
            Ct);
        Result<LoginUserResult> unverified = await handler.Handle(
            new LoginUserCommand { UserIdentifier = "jana", Password = "heslo123" },
            Ct);

        Assert.Equal("User.InvalidCredentials", wrongPassword.Error!.Code);
        Assert.Equal(LoginUserHandler.EmailNotVerified.Code, unverified.Error!.Code);
    }

    [Fact]
    public async Task Login_works_once_verified()
    {
        TestWorld world = new();
        world.AddUser("jana", verified: true);
        FakeAccessTokens tokens = new();
        LoginUserHandler handler = new(world.Users, new FakePasswordHasher(), tokens, tokens);

        Result<LoginUserResult> result = await handler.Handle(
            new LoginUserCommand { UserIdentifier = "jana@example.test", Password = "heslo123" },
            Ct);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Resend_sends_a_new_link_that_replaces_the_old_one_and_respects_the_cooldown()
    {
        TestWorld world = new();
        (User user, _) = world.AddUser("jana", verified: false);
        string oldToken = world.Mailer.IssueToken(user);
        ResendVerificationEmailHandler handler = new(world.Users, world.Mailer);

        await handler.Handle(new ResendVerificationEmailCommand("jana"), Ct);
        Assert.Empty(world.Email.Sent);

        world.Clock.Advance(EmailVerificationMailer.ResendCooldown);
        await handler.Handle(new ResendVerificationEmailCommand("jana"), Ct);
        string newToken = world.Tokens.LastToken;

        Assert.Single(world.Email.Sent);
        Assert.NotEqual(oldToken, newToken);
        Assert.False(world.Mailer.IsValid(user, oldToken));
        Assert.True(world.Mailer.IsValid(user, newToken));
    }

    [Fact]
    public async Task Resend_looks_the_same_for_unknown_and_verified_accounts()
    {
        TestWorld world = new();
        world.AddUser("pepa", verified: true);
        ResendVerificationEmailHandler handler = new(world.Users, world.Mailer);

        Result unknown = await handler.Handle(new ResendVerificationEmailCommand("nikdo"), Ct);
        Result verified = await handler.Handle(new ResendVerificationEmailCommand("pepa"), Ct);

        Assert.True(unknown.IsSuccess);
        Assert.True(verified.IsSuccess);
        Assert.Empty(world.Email.Sent);
        Assert.Equal(0, world.Users.Updates);
    }

    private static VerifyEmailHandler NewVerifyHandler(TestWorld world)
    {
        return new VerifyEmailHandler(
            world.Users,
            world.Profiles,
            world.Mailer,
            world.GrantReward,
            world.Clock,
            world.WalletOptions);
    }
}
