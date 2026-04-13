using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using LanguageExt;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Zagejmi.Contracts.Api.V1.Requests;
using Zagejmi.Contracts.Commands;
using Zagejmi.Shared.Failures;
using Zagejmi.Shared.Models;
using Zagejmi.Write.Application.Abstractions;
using Zagejmi.Write.Domain.Auth;
using Zagejmi.Write.Infrastructure.Ctx;

namespace Zagejmi.Write.Api.Controllers;

/// <summary>
///     Handles authentication and authorization operations for users.
///     Provides endpoints for JWT access-token issuance, refresh rotation, and token revocation.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private static readonly Action<ILogger, string, Exception?> LoginAttemptLogger =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            default,
            "[AuthController] Login attempt for user: {UserIdentifier}");

    private static readonly Action<ILogger, int, Exception?> LoginValidationFailedLogger =
        LoggerMessage.Define<int>(
            LogLevel.Warning,
            default,
            "[AuthController] Login validation failed with {ErrorCount} errors");

    private static readonly Action<ILogger, Exception?> LoginValidationPassedLogger =
        LoggerMessage.Define(
            LogLevel.Information,
            default,
            "[AuthController] Login validation passed, calling handler");

    private static readonly Action<ILogger, string, Exception?> LoginSuccessfulLogger =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            default,
            "[AuthController] Login successful for user: {UserIdentifier}");

    private static readonly Action<ILogger, string, string, Exception?> LoginFailedLogger =
        LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            default,
            "[AuthController] Login failed for user {UserIdentifier}: {Error}");

    private readonly IConfiguration configuration;

    private readonly IHandlerRequest<UserConfirmEmailVerificationCommand, Either<Failure, Unit>>
        confirmEmailVerificationHandler;

    private readonly ILogger<AuthController> logger;

    /// <summary>
    ///     The handler for processing user login commands.
    /// </summary>
    private readonly IHandlerRequest<UserLoginCommand, Either<Failure, string>> loginUserHandler;

    private readonly IHandlerRequest<UserSendEmailVerificationCommand, Either<Failure, Unit>>
        sendEmailVerificationHandler;

    private readonly ITokenService tokenService;
    private readonly IUserRepository userRepository;
    private readonly ZagejmiWriteContext writeContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthController" /> class.
    /// </summary>
    /// <param name="loginUserHandler">The handler for processing login commands.</param>
    /// <param name="sendEmailVerificationHandler">The handler for sending verification emails.</param>
    /// <param name="confirmEmailVerificationHandler">The handler for confirming email verification tokens.</param>
    /// <param name="userRepository">Repository used to resolve users for refresh-token operations.</param>
    /// <param name="tokenService">Service used to issue, parse, and hash auth tokens.</param>
    /// <param name="writeContext">Database context for refresh-token and revocation persistence.</param>
    /// <param name="configuration">Application configuration source for JWT lifetimes.</param>
    /// <param name="logger">The logger instance.</param>
    public AuthController(
        IHandlerRequest<UserLoginCommand, Either<Failure, string>> loginUserHandler,
        IHandlerRequest<UserSendEmailVerificationCommand, Either<Failure, Unit>> sendEmailVerificationHandler,
        IHandlerRequest<UserConfirmEmailVerificationCommand, Either<Failure, Unit>> confirmEmailVerificationHandler,
        IUserRepository userRepository,
        ITokenService tokenService,
        ZagejmiWriteContext writeContext,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        this.loginUserHandler = loginUserHandler;
        this.sendEmailVerificationHandler = sendEmailVerificationHandler;
        this.confirmEmailVerificationHandler = confirmEmailVerificationHandler;
        this.userRepository = userRepository;
        this.tokenService = tokenService;
        this.writeContext = writeContext;
        this.configuration = configuration;
        this.logger = logger;
    }

    /// <summary>
    ///     Authenticates a user and issues an access token with a refresh token.
    /// </summary>
    /// <param name="request">The login request containing user identifier and password.</param>
    /// <returns>
    ///     200 OK with access and refresh tokens when authentication succeeds;
    ///     otherwise 400 Bad Request with validation details.
    /// </returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        LoginAttemptLogger(this.logger, request.UserIdentifier, null);

        // Collect all validation errors
        List<string> errors = [];

        // Validate input
        if (string.IsNullOrWhiteSpace(request.UserIdentifier))
        {
            errors.Add("Username or email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors.Add("Password is required.");
        }

        // Return all errors if any validation failed
        if (errors.Count > 0)
        {
            LoginValidationFailedLogger(this.logger, errors.Count, null);
            return this.BadRequest(new ValidationErrorResponse { Errors = errors });
        }

        LoginValidationPassedLogger(this.logger, null);

        UserLoginCommand userLoginCommand = new() { Username = request.UserIdentifier, Password = request.Password };
        Either<Failure, string> result = await this.loginUserHandler.Handle(
            userLoginCommand,
            this.HttpContext.RequestAborted);

        return await result.Match<Task<IActionResult>>(
            async token =>
            {
                LoginSuccessfulLogger(this.logger, request.UserIdentifier, null);

                if (!this.tokenService.TryGetUserId(token, out Guid userId))
                {
                    return this.StatusCode(
                        500,
                        new ValidationErrorResponse
                        {
                            Errors = ["Could not extract user identity from access token."]
                        });
                }

                string refreshToken = this.tokenService.GenerateRefreshToken();
                DateTimeOffset nowUtc = DateTimeOffset.UtcNow;

                this.writeContext.RefreshTokens.Add(
                    new RefreshTokenRecord
                    {
                        Id = Guid.CreateVersion7(),
                        UserId = userId,
                        TokenFamilyId = Guid.CreateVersion7(),
                        TokenHash = this.tokenService.HashRefreshToken(refreshToken),
                        CreatedAtUtc = nowUtc,
                        ExpiresAtUtc = nowUtc.AddDays(this.GetRefreshTokenLifetimeDays())
                    });

                await this.writeContext.SaveChangesAsync(this.HttpContext.RequestAborted);

                return this.Ok(new { accessToken = token, refreshToken });
            },
            failure =>
            {
                LoginFailedLogger(this.logger, request.UserIdentifier, failure.Message, null);
                return Task.FromResult<IActionResult>(
                    this.BadRequest(new ValidationErrorResponse { Errors = [failure.Message] }));
            });
    }

    /// <summary>
    ///     Exchanges a valid refresh token for a new access token and rotates the refresh token.
    /// </summary>
    /// <param name="request">The refresh-token payload.</param>
    /// <returns>
    ///     200 OK with a new access/refresh pair when successful;
    ///     400 for invalid input; 401 when the refresh token is invalid, revoked, or expired.
    /// </returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return this.BadRequest(new ValidationErrorResponse { Errors = ["Refresh token is required."] });
        }

        string presentedHash = this.tokenService.HashRefreshToken(request.RefreshToken);
        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;

        // Keep rotation atomic so old and new refresh-token states cannot diverge.
        await using IDbContextTransaction transaction =
            await this.writeContext.Database.BeginTransactionAsync(this.HttpContext.RequestAborted);

        RefreshTokenRecord? currentToken = await this.writeContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == presentedHash, this.HttpContext.RequestAborted);

        if (currentToken == null || currentToken.RevokedAtUtc.HasValue || currentToken.ExpiresAtUtc <= nowUtc)
        {
            return this.Unauthorized(new ValidationErrorResponse { Errors = ["Invalid refresh token."] });
        }

        User? user =
            await this.userRepository.GetByIdAsync(currentToken.UserId, this.HttpContext.RequestAborted);
        if (user == null)
        {
            return this.Unauthorized(
                new ValidationErrorResponse { Errors = ["User for refresh token no longer exists."] });
        }

        string newAccessToken = this.tokenService.GenerateToken(user);
        string newRefreshToken = this.tokenService.GenerateRefreshToken();

        RefreshTokenRecord replacement = new()
        {
            Id = Guid.CreateVersion7(),
            UserId = currentToken.UserId,
            TokenFamilyId = currentToken.TokenFamilyId,
            TokenHash = this.tokenService.HashRefreshToken(newRefreshToken),
            CreatedAtUtc = nowUtc,
            ExpiresAtUtc = nowUtc.AddDays(this.GetRefreshTokenLifetimeDays())
        };

        currentToken.RevokedAtUtc = nowUtc;
        currentToken.ReplacedByTokenId = replacement.Id;
        currentToken.RevocationReason = "rotated";

        this.writeContext.RefreshTokens.Add(replacement);
        await this.writeContext.SaveChangesAsync(this.HttpContext.RequestAborted);
        await transaction.CommitAsync(this.HttpContext.RequestAborted);

        return this.Ok(new { accessToken = newAccessToken, refreshToken = newRefreshToken });
    }

    /// <summary>
    ///     Revokes refresh token(s) for the authenticated user and blacklists the current access token JTI.
    /// </summary>
    /// <param name="request">Revocation request defining single-token or all-session scope.</param>
    /// <returns>
    ///     200 OK on successful revocation; 400 for invalid payload; 401 for invalid caller identity; 404 when token is not
    ///     found.
    /// </returns>
    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke([FromBody] RevokeTokenRequest request)
    {
        string? subject = this.User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                          this.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return this.Unauthorized(new ValidationErrorResponse { Errors = ["Invalid user identity."] });
        }

        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;

        if (request.RevokeAllSessions)
        {
            List<RefreshTokenRecord> userRefreshTokens = await this.writeContext.RefreshTokens
                .Where(x => x.UserId == userId && !x.RevokedAtUtc.HasValue && x.ExpiresAtUtc > nowUtc)
                .ToListAsync(this.HttpContext.RequestAborted);

            foreach (RefreshTokenRecord token in userRefreshTokens)
            {
                token.RevokedAtUtc = nowUtc;
                token.RevocationReason = "manual-revoke-all";
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return this.BadRequest(
                    new ValidationErrorResponse
                    {
                        Errors = ["Refresh token is required when revoke-all is false."]
                    });
            }

            string tokenHash = this.tokenService.HashRefreshToken(request.RefreshToken);
            RefreshTokenRecord? tokenToRevoke = await this.writeContext.RefreshTokens
                .FirstOrDefaultAsync(
                    x => x.UserId == userId && x.TokenHash == tokenHash,
                    this.HttpContext.RequestAborted);

            if (tokenToRevoke == null)
            {
                return this.NotFound(new ValidationErrorResponse { Errors = ["Refresh token not found."] });
            }

            tokenToRevoke.RevokedAtUtc = nowUtc;
            tokenToRevoke.RevocationReason = "manual-revoke";
        }

        string? jti = this.User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        if (!string.IsNullOrWhiteSpace(jti))
        {
            // Persist current access token revocation so JWT middleware can reject it immediately.
            DateTimeOffset expiresAtUtc = ResolveAccessTokenExpiry(this.User, nowUtc.AddMinutes(5));
            bool alreadyRevoked = await this.writeContext.RevokedAccessTokens
                .AnyAsync(x => x.JwtId == jti, this.HttpContext.RequestAborted);

            if (!alreadyRevoked)
            {
                this.writeContext.RevokedAccessTokens.Add(
                    new RevokedAccessTokenRecord
                    {
                        Id = Guid.CreateVersion7(),
                        UserId = userId,
                        JwtId = jti,
                        RevokedAtUtc = nowUtc,
                        ExpiresAtUtc = expiresAtUtc,
                        Reason = request.RevokeAllSessions ? "manual-revoke-all" : "manual-revoke"
                    });
            }
        }

        await this.writeContext.SaveChangesAsync(this.HttpContext.RequestAborted);
        return this.Ok();
    }

    /// <summary>
    ///     Placeholder logout endpoint for token-based clients.
    /// </summary>
    /// <returns>
    ///     200 OK.
    /// </returns>
    /// <remarks>
    ///     JWT sessions are effectively ended through <c>/revoke</c>; clients may use this endpoint as a UI logout trigger.
    /// </remarks>
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        await Task.CompletedTask;
        return this.Ok();
    }

    /// <summary>
    ///     Requests a verification email for the specified user identifier.
    /// </summary>
    /// <param name="command">Verification email command payload.</param>
    /// <returns>200 OK when accepted, or 400 Bad Request with validation details.</returns>
    [HttpPost("verify-email/send")]
    [AllowAnonymous]
    public async Task<IActionResult> SendVerificationEmail([FromBody] UserSendEmailVerificationCommand command)
    {
        Either<Failure, Unit> result =
            await this.sendEmailVerificationHandler.Handle(command, this.HttpContext.RequestAborted);

        return result.Match<IActionResult>(
            _ => this.Ok(new { message = "Verification email sent." }),
            failure => this.BadRequest(new ValidationErrorResponse { Errors = [failure.Message] }));
    }

    /// <summary>
    ///     Confirms ownership of an email address using verification token.
    /// </summary>
    /// <param name="userId">User aggregate ID from query string.</param>
    /// <param name="token">Verification token from query string.</param>
    /// <returns>Plain-text success message or 400 Bad Request when token is invalid/expired.</returns>
    [HttpGet("verify-email/confirm")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmVerificationEmail([FromQuery] Guid userId, [FromQuery] string token)
    {
        UserConfirmEmailVerificationCommand command = new() { UserId = userId, Token = token };
        Either<Failure, Unit> result =
            await this.confirmEmailVerificationHandler.Handle(command, this.HttpContext.RequestAborted);

        return result.Match<IActionResult>(
            _ => this.Content("Email successfully verified. You can now log in.", "text/plain"),
            failure => this.BadRequest(new ValidationErrorResponse { Errors = [failure.Message] }));
    }

    private static DateTimeOffset ResolveAccessTokenExpiry(ClaimsPrincipal principal, DateTimeOffset fallback)
    {
        string? expClaim = principal.FindFirstValue(JwtRegisteredClaimNames.Exp);
        if (long.TryParse(expClaim, out long unixSeconds))
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        }

        return fallback;
    }

    private int GetRefreshTokenLifetimeDays()
    {
        return int.TryParse(this.configuration["Jwt:RefreshTokenDays"], out int days) && days > 0 ? days : 30;
    }
}