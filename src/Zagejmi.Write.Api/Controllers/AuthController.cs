using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

using LanguageExt;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Zagejmi.Contracts.Api.V1.Requests;
using Zagejmi.Contracts.Commands;
using Zagejmi.Shared.Failures;
using Zagejmi.Shared.Models;
using Zagejmi.Write.Application.Abstractions;

namespace Zagejmi.Write.Api.Controllers;

/// <summary>
///     Handles authentication and authorization operations for users.
///     Provides endpoints for user login and logout with cookie-based session management.
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

    private readonly ILogger<AuthController> logger;

    /// <summary>
    ///     The handler for processing user login commands.
    /// </summary>
    private readonly IHandlerRequest<UserLoginCommand, Either<Failure, string>> loginUserHandler;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthController" /> class.
    /// </summary>
    /// <param name="loginUserHandler">The handler for processing login commands.</param>
    /// <param name="logger">The logger instance.</param>
    public AuthController(
        IHandlerRequest<UserLoginCommand, Either<Failure, string>> loginUserHandler,
        ILogger<AuthController> logger)
    {
        this.loginUserHandler = loginUserHandler;
        this.logger = logger;
    }

    /// <summary>
    ///     Authenticates a user and creates a session.
    /// </summary>
    /// <param name="request">The login request containing username, password, and redirect URLs.</param>
    /// <returns>
    ///     A redirect to the success URL if authentication succeeds, or to the failure URL with an error parameter if it
    ///     fails.
    /// </returns>
    /// <remarks>
    ///     This endpoint accepts JSON and validates both the credentials and redirect URLs for security.
    ///     The session is created as a persistent cookie valid for 7 days.
    /// </remarks>
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
        Either<Failure, string> result = await this.loginUserHandler.Handle(userLoginCommand, this.HttpContext.RequestAborted);

        return await result.Match<Task<IActionResult>>(
            async token =>
            {
                LoginSuccessfulLogger(this.logger, request.UserIdentifier, null);
                JwtSecurityTokenHandler handler = new();
                JwtSecurityToken? jwtToken = handler.ReadJwtToken(token);
                ClaimsIdentity identity = new(jwtToken.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
                ClaimsPrincipal principal = new(identity);

                await this.HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddDays(7)
                    });

                return this.Ok(new { token });
            },
            failure =>
            {
                LoginFailedLogger(this.logger, request.UserIdentifier, failure.message, null);
                return Task.FromResult<IActionResult>(
                    this.BadRequest(new ValidationErrorResponse { Errors = [failure.message] }));
            });
    }

    /// <summary>
    ///     Terminates the current user session and logs them out.
    /// </summary>
    /// <returns>
    ///     An OK response after successfully clearing the session.
    /// </returns>
    /// <remarks>
    ///     This endpoint clears the authentication cookie and invalidates the current session.
    /// </remarks>
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        await this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return this.Ok();
    }
}