using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Zagejmi.Application.Abstractions;

namespace Zagejmi.Api.Controllers;

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

    private readonly IHandlerRequest<UserConfirmEmailVerificationCommand, (Guid? Result, Failure? Error)>
        confirmEmailVerificationHandler;

    private readonly ILogger<AuthController> logger;

    /// <summary>
    ///     The handler for processing user login commands.
    /// </summary>
    private readonly IHandlerRequest<UserLoginCommand, (Guid? Result, Failure? Error)> loginUserHandler;

    private readonly IHandlerRequest<UserSendEmailVerificationCommand, (Guid? Result, Failure? Error)>
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
        IHandlerRequest<UserLoginCommand, (Guid? Result, Failure? Error)> loginUserHandler,
        IHandlerRequest<UserSendEmailVerificationCommand, (Guid? Result, Failure? Error)> sendEmailVerificationHandler,
        IHandlerRequest<UserConfirmEmailVerificationCommand, (Guid? Result, Failure? Error)> confirmEmailVerificationHandler,
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
        LoginAttemptLogger(logger, request.UserIdentifier, null);

        // Collect all validation errors
        List<string> errors = new();

        // Validate input
        if (string.IsNullOrWhiteSpace(request.UserIdentifier)) errors.Add("Username or email is required.");

        if (string.IsNullOrWhiteSpace(request.Password)) errors.Add("Password is required.");

        // Return all errors if any validation failed
        if (errors.Count > 0)
        {
            LoginValidationFailedLogger(logger, errors.Count, null);
            return this.BadRequest(new ValidationErrorResponse { Errors = errors });
        }

        LoginValidationPassedLogger(logger, null);

        UserLoginCommand userLoginCommand = new() { Username = request.UserIdentifier, Password = request.Password };
        (Guid? Result, Failure? Error) result = await loginUserHandler.Handle(
            userLoginCommand,
            HttpContext.RequestAborted);

        return await result.Match<Task<IActionResult>>(
            async token =>
            {
                LoginSuccessfulLogger(logger, request.UserIdentifier, null);

                if (!tokenService.TryGetUserId(token, out Guid userId))
                    return this.StatusCode(
                        500,
                        new ValidationErrorResponse
                        {
                            Errors = new() {"Could not extract user identity from access token."],
                        });

                string refreshToken = tokenService.GenerateRefreshToken();
                DateTimeOffset nowUtc = DateTimeOffset.UtcNow;

                writeContext.RefreshTokens.Add(
                    new RefreshTokenRecord
                    {
                        Id = Guid.CreateVersion7(),
                        UserId = userId,
                        TokenFamilyId = Guid.CreateVersion7(),
                        TokenHash = tokenService.HashRefreshToken(refreshToken),
                        CreatedAtUtc = nowUtc,
                        ExpiresAtUtc = nowUtc.AddDays(GetRefreshTokenLifetimeDays()),
                    });

                await writeContext.SaveChangesAsync(HttpContext.RequestAborted);

                return Ok(new { accessToken = token, refreshToken });
            },
            failure =>
            {
                LoginFailedLogger(logger, request.UserIdentifier, failure.Message, null);
                return Task.FromResult<IActionResult>(
                    this.BadRequest(new ValidationErrorResponse { Errors = new() {failure.Message] }));
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
            return this.BadRequest(new ValidationErrorResponse { Errors = new() {"Refresh token is required."] });

        string presentedHash = tokenService.HashRefreshToken(request.RefreshToken);
        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;

        // Keep rotation atomic so old and new refresh-token states cannot diverge.
        await using IDbContextTransaction transaction =
            await writeContext.Database.BeginTransactionAsync(HttpContext.RequestAborted);

        RefreshTokenRecord? currentToken = await writeContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == presentedHash, HttpContext.RequestAborted);

        if (currentToken == null || currentToken.RevokedAtUtc.HasValue || currentToken.ExpiresAtUtc <= nowUtc)
            return this.Unauthorized(new ValidationErrorResponse { Errors = new() {"Invalid refresh token."] });

        User? user =
            await userRepository.GetByIdAsync(currentToken.UserId, HttpContext.RequestAborted);
        if (user == null)
            return this.Unauthorized(
                new ValidationErrorResponse { Errors = new() {"User for refresh token no longer exists."] });

        string newAccessToken = tokenService.GenerateToken(user);
        string newRefreshToken = tokenService.GenerateRefreshToken();

        RefreshTokenRecord replacement = new()
        {
            Id = Guid.CreateVersion7(),
            UserId = currentToken.UserId,
            TokenFamilyId = currentToken.TokenFamilyId,
            TokenHash = tokenService.HashRefreshToken(newRefreshToken),
            CreatedAtUtc = nowUtc,
            ExpiresAtUtc = nowUtc.AddDays(GetRefreshTokenLifetimeDays()),
        };

        currentToken.RevokedAtUtc = nowUtc;
        currentToken.ReplacedByTokenId = replacement.Id;
        currentToken.RevocationReason = "rotated";

        writeContext.RefreshTokens.Add(replacement);
        await writeContext.SaveChangesAsync(HttpContext.RequestAborted);
        await transaction.CommitAsync(HttpContext.RequestAborted);

        return Ok(new { accessToken = newAccessToken, refreshToken = newRefreshToken });
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
        string? subject = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                          User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
            return this.Unauthorized(new ValidationErrorResponse { Errors = new() {"Invalid user identity."] });

        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;

        if (request.RevokeAllSessions)
        {
            List<RefreshTokenRecord> userRefreshTokens = await writeContext.RefreshTokens
                .Where(x => x.UserId == userId && !x.RevokedAtUtc.HasValue && x.ExpiresAtUtc > nowUtc)
                .ToListAsync(HttpContext.RequestAborted);

            foreach (RefreshTokenRecord token in userRefreshTokens)
            {
                token.RevokedAtUtc = nowUtc;
                token.RevocationReason = "manual-revoke-all";
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return this.BadRequest(
                    new ValidationErrorResponse
                    {
                        Errors = new() {"Refresh token is required when revoke-all is false."],
                    });

            string tokenHash = tokenService.HashRefreshToken(request.RefreshToken);
            RefreshTokenRecord? tokenToRevoke = await writeContext.RefreshTokens
                .FirstOrDefaultAsync(
                    x => x.UserId == userId && x.TokenHash == tokenHash,
                    HttpContext.RequestAborted);

            if (tokenToRevoke == null)
                return this.NotFound(new ValidationErrorResponse { Errors = new() {"Refresh token not found."] });

            tokenToRevoke.RevokedAtUtc = nowUtc;
            tokenToRevoke.RevocationReason = "manual-revoke";
        }

        string? jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        if (!string.IsNullOrWhiteSpace(jti))
        {
            // Persist current access token revocation so JWT middleware can reject it immediately.
            DateTimeOffset expiresAtUtc = ResolveAccessTokenExpiry(User, nowUtc.AddMinutes(5));
            bool alreadyRevoked = await writeContext.RevokedAccessTokens
                .AnyAsync(x => x.JwtId == jti, HttpContext.RequestAborted);

            if (!alreadyRevoked)
                writeContext.RevokedAccessTokens.Add(
                    new RevokedAccessTokenRecord
                    {
                        Id = Guid.CreateVersion7(),
                        UserId = userId,
                        JwtId = jti,
                        RevokedAtUtc = nowUtc,
                        ExpiresAtUtc = expiresAtUtc,
                        Reason = request.RevokeAllSessions ? "manual-revoke-all" : "manual-revoke",
                    });
        }

        await writeContext.SaveChangesAsync(HttpContext.RequestAborted);
        return Ok();
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
        return Ok();
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
        (Guid? Result, Failure? Error) result =
            await sendEmailVerificationHandler.Handle(command, HttpContext.RequestAborted);

        return result.Match<IActionResult>(
            _ => Ok(new { message = "Verification email sent." }),
            failure => this.BadRequest(new ValidationErrorResponse { Errors = new() {failure.Message] }));
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
        (Guid? Result, Failure? Error) result =
            await confirmEmailVerificationHandler.Handle(command, HttpContext.RequestAborted);

        return result.Match<IActionResult>(
            _ => Content("Email successfully verified. You can now log in.", "text/plain"),
            failure => this.BadRequest(new ValidationErrorResponse { Errors = new() {failure.Message] }));
    }

    private static DateTimeOffset ResolveAccessTokenExpiry(ClaimsPrincipal principal, DateTimeOffset fallback)
    {
        string? expClaim = principal.FindFirstValue(JwtRegisteredClaimNames.Exp);
        if (long.TryParse(expClaim, out long unixSeconds)) return DateTimeOffset.FromUnixTimeSeconds(unixSeconds);

        return fallback;
    }

    private int GetRefreshTokenLifetimeDays()
    {
        return int.TryParse(configuration["Jwt:RefreshTokenDays"], out int days) && days > 0 ? days : 30;
    }
}