using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Zagejmi.Read.Api.Models.Requests;
using Zagejmi.Shared.Models;

namespace Zagejmi.Read.Api.Controllers;

/// <summary>
///     Handles authentication and authorization operations for users.
///     Acts as a proxy to the Write.Api for authentication requests.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    /// <summary>
    ///     Logs missing Write.Api base address configuration.
    /// </summary>
    private static readonly Action<ILogger, Exception?> WriteApiConfigurationMissingLogger =
        LoggerMessage.Define(
            LogLevel.Error,
            default,
            "WriteApi:BaseAddress configuration is missing");

    /// <summary>
    ///     Logs the outgoing Write.Api login URL.
    /// </summary>
    private static readonly Action<ILogger, string, Exception?> ForwardingLoginLogger =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            default,
            "Forwarding login to: {FullUrl}");

    /// <summary>
    ///     Logs unhandled login processing failures.
    /// </summary>
    private static readonly Action<ILogger, Exception?> LoginErrorLogger =
        LoggerMessage.Define(
            LogLevel.Error,
            default,
            "Login error occurred");

    /// <summary>
    ///     Logs logout requests.
    /// </summary>
    private static readonly Action<ILogger, Exception?> LogoutRequestedLogger =
        LoggerMessage.Define(
            LogLevel.Information,
            default,
            "Logout requested");

    /// <summary>
    ///     Logs successful sign-out operations.
    /// </summary>
    private static readonly Action<ILogger, Exception?> UserSignedOutLogger =
        LoggerMessage.Define(
            LogLevel.Information,
            default,
            "User signed out successfully");

    /// <summary>
    ///     Logs unhandled logout processing failures.
    /// </summary>
    private static readonly Action<ILogger, Exception> LogoutErrorLogger =
        LoggerMessage.Define(
            LogLevel.Error,
            default,
            "Logout error occurred");

    /// <summary>
    ///     Logs failures while parsing or processing a returned JWT token.
    /// </summary>
    private static readonly Action<ILogger, Exception> JwtProcessingFailedLogger =
        LoggerMessage.Define(
            LogLevel.Error,
            default,
            "Failed to process JWT token");

    /// <summary>
    ///     Provides application configuration values.
    /// </summary>
    private readonly IConfiguration configuration;

    /// <summary>
    ///     The HTTP client for communicating with the Write.Api.
    /// </summary>
    private readonly HttpClient httpClient;

    /// <summary>
    ///     The logger used for authentication diagnostics.
    /// </summary>
    private readonly ILogger<AuthController> logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthController" /> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="logger">The logger instance.</param>
    public AuthController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        this.httpClient = httpClientFactory.CreateClient();
        this.configuration = configuration;
        this.logger = logger;
    }

    /// <summary>
    ///     Authenticates a user by forwarding the request to the Write.Api.
    /// </summary>
    /// <param name="request">The login payload containing user credentials and optional redirect URLs.</param>
    /// <returns>
    ///     A JSON payload from Write.Api on success, or a normalized validation error response on failure.
    /// </returns>
    /// <remarks>
    ///     This endpoint proxies to the Write.Api's authentication service.
    ///     The session is created as a persistent cookie valid for 7 days.
    /// </remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    [Consumes("application/json")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            (bool isSuccess, string responseContent, ValidationErrorResponse errorResponse) =
                await this.ForwardLoginAndSignIn(request);

            if (isSuccess)
            {
                return this.Content(responseContent, "application/json");
            }

            return this.BadRequest(errorResponse);
        }
        catch (Exception ex)
        {
            LoginErrorLogger(this.logger, ex);
            return this.BadRequest(new ValidationErrorResponse { Errors = [$"An error occurred: {ex.Message}"] });
        }
    }

    /// <summary>
    ///     Logs out the current user by clearing the authentication cookie.
    /// </summary>
    /// <returns>
    ///     An OK response after successfully logging out.
    /// </returns>
    /// <remarks>
    ///     This endpoint clears the authentication cookie for the user.
    /// </remarks>
    [HttpGet("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        try
        {
            LogoutRequestedLogger(this.logger, null);

            // Sign out on cookie authentication
            await this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            UserSignedOutLogger(this.logger, null);

            // Redirect to home after logout
            return this.Redirect("/");
        }
        catch (Exception ex)
        {
            LogoutErrorLogger(this.logger, ex);

            // Even on error, redirect to home
            return this.Redirect("/");
        }
    }

    /// <summary>
    ///     Forwards credentials to Write.Api and signs in the current Read.Api user with cookie authentication.
    /// </summary>
    /// <param name="request">The incoming login request.</param>
    /// <returns>
    ///     A tuple containing a success flag, raw Write.Api response payload, and a normalized validation error response.
    /// </returns>
    private async Task<(bool IsSuccess, string ResponseContent, ValidationErrorResponse ErrorResponse)>
        ForwardLoginAndSignIn(LoginRequest request)
    {
        string? writeApiBaseAddress = this.configuration["WriteApi:BaseAddress"];
        if (string.IsNullOrWhiteSpace(writeApiBaseAddress))
        {
            WriteApiConfigurationMissingLogger(this.logger, null);
            return (false, string.Empty, new ValidationErrorResponse
            {
                Errors = ["Server configuration error: Write API address not configured"]
            });
        }

        if (!writeApiBaseAddress.EndsWith('/'))
        {
            writeApiBaseAddress += "/";
        }

        string fullUrl = $"{writeApiBaseAddress}api/v1/auth/login";
        ForwardingLoginLogger(this.logger, fullUrl, null);

        string jsonContent = JsonSerializer.Serialize(request);
        using StringContent content = new(jsonContent, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await this.httpClient.PostAsync(fullUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync();
            ValidationErrorResponse? errorResponse = JsonSerializer.Deserialize<ValidationErrorResponse>(errorContent);
            return (false, string.Empty,
                errorResponse ?? new ValidationErrorResponse { Errors = ["Authentication failed"] });
        }

        string responseContent = await response.Content.ReadAsStringAsync();

        try
        {
            JsonElement result = JsonSerializer.Deserialize<JsonElement>(responseContent);
            if (result.TryGetProperty("token", out JsonElement tokenElement))
            {
                string? jwtToken = tokenElement.GetString();
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    JwtSecurityTokenHandler handler = new();

                    if (handler.ReadToken(jwtToken) is JwtSecurityToken jsonToken)
                    {
                        ClaimsIdentity identity = new(
                            jsonToken.Claims,
                            CookieAuthenticationDefaults.AuthenticationScheme);
                        ClaimsPrincipal principal = new(identity);

                        await this.HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            principal,
                            new AuthenticationProperties
                                { IsPersistent = true, ExpiresUtc = DateTime.UtcNow.AddDays(7) });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            JwtProcessingFailedLogger(this.logger, ex);
        }

        return (true, responseContent, new ValidationErrorResponse());
    }
}