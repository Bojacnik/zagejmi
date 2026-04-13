using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Zagejmi.Contracts.Api.V1.Requests;
using Zagejmi.Contracts.Commands;
using Zagejmi.Read.Api.Models.Requests;
using Zagejmi.Shared.Models;

using LoginRequest = Zagejmi.Read.Api.Models.Requests.LoginRequest;

namespace Zagejmi.Read.Api.Controllers;

/// <summary>
///     Handles authentication and authorization operations for users.
///     Auth token lifecycle is delegated to Write.Api; this controller acts as a read-side auth proxy.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    /// <summary>
    ///     Logger delegate for unexpected login/proxy failures.
    /// </summary>
    private static readonly Action<ILogger, Exception?> LoginErrorLogger =
        LoggerMessage.Define(
            LogLevel.Error,
            default,
            "Login error occurred");

    /// <summary>
    ///     Logger delegate for logout request entry.
    /// </summary>
    private static readonly Action<ILogger, Exception?> LogoutRequestedLogger =
        LoggerMessage.Define(
            LogLevel.Information,
            default,
            "Logout requested");

    /// <summary>
    ///     Logger delegate for successful logout completion.
    /// </summary>
    private static readonly Action<ILogger, Exception?> UserSignedOutLogger =
        LoggerMessage.Define(
            LogLevel.Information,
            default,
            "User signed out successfully");

    /// <summary>
    ///     Named HTTP client targeting Write.Api.
    /// </summary>
    private readonly HttpClient httpClient;

    /// <summary>
    ///     Logger for controller diagnostics.
    /// </summary>
    private readonly ILogger<AuthController> logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthController" /> class.
    /// </summary>
    /// <param name="httpClientFactory">Factory used to resolve named HTTP clients.</param>
    /// <param name="logger">Logger instance for auth proxy operations.</param>
    public AuthController(
        IHttpClientFactory httpClientFactory,
        ILogger<AuthController> logger)
    {
        this.httpClient = httpClientFactory.CreateClient("WriteAPI");
        this.logger = logger;
    }

    /// <summary>
    ///     Authenticates user credentials by delegating to Write.Api and returns JWT tokens.
    /// </summary>
    /// <param name="request">Login payload containing user identifier and password.</param>
    /// <returns>Write.Api login response pass-through (access token + refresh token or validation errors).</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [Consumes("application/json")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            HttpResponseMessage response = await this.httpClient.PostAsJsonAsync(
                "api/v1/auth/login",
                new Contracts.Api.V1.Requests.LoginRequest
                {
                    UserIdentifier = request.UserIdentifier,
                    Password = request.Password
                });

            string body = await response.Content.ReadAsStringAsync(this.HttpContext.RequestAborted);
            return this.StatusCode((int)response.StatusCode, DeserializeOrRaw(body));
        }
        catch (Exception ex)
        {
            LoginErrorLogger(this.logger, ex);
            return this.BadRequest(new ValidationErrorResponse { Errors = [$"An error occurred: {ex.Message}"] });
        }
    }

    /// <summary>
    ///     Deprecated browser form endpoint in pure-JWT mode.
    /// </summary>
    /// <param name="userIdentifier">Username or email.</param>
    /// <param name="password">Plaintext password.</param>
    /// <param name="returnUrl">Optional local return URL.</param>
    /// <returns>410 Gone with migration guidance.</returns>
    [HttpPost("login/browser")]
    [AllowAnonymous]
    [Consumes("application/x-www-form-urlencoded")]
    public Task<IActionResult> LoginBrowser(
        [FromForm] string userIdentifier,
        [FromForm] string password,
        [FromForm] string? returnUrl)
    {
        _ = userIdentifier;
        _ = password;
        _ = returnUrl;

        return Task.FromResult<IActionResult>(
            this.StatusCode(
                StatusCodes.Status410Gone,
                new ValidationErrorResponse
                {
                    Errors = ["Browser form login is disabled in pure JWT mode. Use POST /api/v1/auth/login."]
                }));
    }

    /// <summary>
    ///     Exchanges refresh token for a new access/refresh pair by delegating to Write.Api.
    /// </summary>
    /// <returns>Write.Api refresh response pass-through.</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [Consumes("application/json")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        HttpResponseMessage response = await this.httpClient.PostAsJsonAsync("api/v1/auth/refresh", request);
        string body = await response.Content.ReadAsStringAsync(this.HttpContext.RequestAborted);
        return this.StatusCode((int)response.StatusCode, DeserializeOrRaw(body));
    }

    /// <summary>
    ///     Revokes refresh token/session by delegating to Write.Api.
    /// </summary>
    /// <returns>Write.Api revoke response pass-through.</returns>
    [HttpPost("revoke")]
    [Authorize]
    [Consumes("application/json")]
    public async Task<IActionResult> Revoke([FromBody] RevokeTokenRequest request)
    {
        HttpRequestMessage proxyRequest = new(HttpMethod.Post, "api/v1/auth/revoke")
        {
            Content = JsonContent.Create(request)
        };

        string? authorization = this.HttpContext.Request.Headers.Authorization;
        if (!string.IsNullOrWhiteSpace(authorization))
        {
            proxyRequest.Headers.TryAddWithoutValidation("Authorization", authorization);
        }

        HttpResponseMessage response = await this.httpClient.SendAsync(proxyRequest, this.HttpContext.RequestAborted);
        string body = await response.Content.ReadAsStringAsync(this.HttpContext.RequestAborted);
        return this.StatusCode((int)response.StatusCode, DeserializeOrRaw(body));
    }

    /// <summary>
    ///     Sends a verification email for an existing account.
    /// </summary>
    /// <param name="request">The user identifier payload.</param>
    /// <returns>OK when accepted, or validation errors when request fails.</returns>
    [HttpPost("verify-email/send")]
    [AllowAnonymous]
    [Consumes("application/json")]
    public async Task<IActionResult> SendVerificationEmail([FromBody] SendVerificationEmailRequest request)
    {
        UserSendEmailVerificationCommand command = new() { UserIdentifier = request.UserIdentifier };
        HttpResponseMessage response = await this.httpClient.PostAsJsonAsync("api/v1/auth/verify-email/send", command);
        string responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return this.Ok(new { message = "Verification email sent. Check your inbox." });
        }

        ValidationErrorResponse? errors = JsonSerializer.Deserialize<ValidationErrorResponse>(responseBody);
        return this.BadRequest(
            errors ?? new ValidationErrorResponse { Errors = ["Unable to send verification email."] });
    }

    /// <summary>
    ///     Confirms email ownership with user ID and token.
    /// </summary>
    /// <param name="userId">The user aggregate ID.</param>
    /// <param name="token">The email verification token.</param>
    /// <returns>OK when confirmed, or validation errors when invalid/expired.</returns>
    [HttpGet("verify-email/confirm")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmVerificationEmail([FromQuery] string userId, [FromQuery] string token)
    {
        string fullUrl =
            $"api/v1/auth/verify-email/confirm?userId={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}";

        HttpResponseMessage response = await this.httpClient.GetAsync(fullUrl);
        string responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return this.Ok(new { message = "Email verified successfully." });
        }

        ValidationErrorResponse? errors = JsonSerializer.Deserialize<ValidationErrorResponse>(responseBody);
        return this.BadRequest(errors ?? new ValidationErrorResponse { Errors = ["Unable to verify email."] });
    }

    /// <summary>
    ///     Logs out current JWT client by forwarding to Write.Api and returning a neutral response.
    /// </summary>
    /// <returns>200 OK. Client should clear local auth state/tokens.</returns>
    [HttpPost("logout")]
    [HttpGet("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        LogoutRequestedLogger(this.logger, null);
        try
        {
            await this.httpClient.PostAsync("api/v1/auth/logout", null);
        }
        catch
        {
            // Logout is idempotent; client-side token cleanup remains the source of truth.
        }

        UserSignedOutLogger(this.logger, null);
        return this.Ok(new { message = "Logged out. Remove stored tokens on the client." });
    }

    /// <summary>
    ///     Attempts to deserialize proxied JSON responses while preserving non-JSON bodies as plain detail payloads.
    /// </summary>
    /// <param name="body">Raw response body from proxied request.</param>
    /// <returns>Deserialized JSON element when possible, otherwise a JSON object containing fallback detail text.</returns>
    private static JsonElement DeserializeOrRaw(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return JsonSerializer.SerializeToElement(new { });
        }

        try
        {
            return JsonSerializer.Deserialize<JsonElement>(body);
        }
        catch
        {
            return JsonSerializer.SerializeToElement(new { detail = body });
        }
    }
}