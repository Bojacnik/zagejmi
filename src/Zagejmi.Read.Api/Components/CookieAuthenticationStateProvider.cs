using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Zagejmi.Read.Api.Components;

/// <summary>
///     Custom AuthenticationStateProvider that integrates with server-side cookie authentication
///     for Blazor InteractiveServer components.
/// </summary>
public class CookieAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly Action<ILogger, Exception?> HttpContextIsNullLogger =
        LoggerMessage.Define(
            LogLevel.Warning,
            default,
            "[AuthStateProvider] HttpContext is null");

    private static readonly Action<ILogger, bool, Exception?> AuthStateLogger =
        LoggerMessage.Define<bool>(
            LogLevel.Information,
            default,
            "[AuthStateProvider] HttpContext available. IsAuthenticated: {IsAuthenticated}");

    private static readonly Action<ILogger, string?, string?, Exception?> IdentityLogger =
        LoggerMessage.Define<string?, string?>(
            LogLevel.Information,
            default,
            "[AuthStateProvider] Identity AuthenticationType: {AuthType}, Name: {IdentityName}");

    private static readonly Action<ILogger, int, Exception?> ClaimCountLogger =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            default,
            "[AuthStateProvider] User has {ClaimCount} claims");

    private static readonly Action<ILogger, string, string, Exception?> ClaimLogger =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            default,
            "[AuthStateProvider] Claim - Type: {ClaimType}, Value: {ClaimValue}");

    private static readonly Action<ILogger, Exception?> NoClaimsLogger =
        LoggerMessage.Define(
            LogLevel.Information,
            default,
            "[AuthStateProvider] User has no claims");

    private static readonly Action<ILogger, Exception> AuthStateExceptionLogger =
        LoggerMessage.Define(
            LogLevel.Error,
            default,
            "[AuthStateProvider] Exception in GetAuthenticationStateAsync");

    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ILogger<CookieAuthenticationStateProvider> logger;
    private ClaimsPrincipal? currentUser;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CookieAuthenticationStateProvider" /> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <param name="logger">The logger instance.</param>
    public CookieAuthenticationStateProvider(
        IHttpContextAccessor httpContextAccessor,
        ILogger<CookieAuthenticationStateProvider> logger)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.logger = logger;
    }

    /// <summary>
    ///     Gets the authentication state asynchronously from the server-side HttpContext.
    /// </summary>
    /// <returns>A task representing the authentication state.</returns>
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            if (this.currentUser != null)
            {
                return Task.FromResult(new AuthenticationState(this.currentUser));
            }

            HttpContext? httpContext = this.httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                HttpContextIsNullLogger(this.logger, null);
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            }

            ClaimsPrincipal user = httpContext.User;
            bool isAuthenticated = user.Identity?.IsAuthenticated ?? false;

            AuthStateLogger(this.logger, isAuthenticated, null);

            if (user.Identity != null)
            {
                IdentityLogger(this.logger, user.Identity.AuthenticationType, user.Identity.Name, null);
            }

            if (!this.logger.IsEnabled(LogLevel.Information))
            {
                return Task.FromResult(new AuthenticationState(user));
            }

            Claim[] claims = user.Claims.Take(10).ToArray();
            if (claims.Length > 0)
            {
                ClaimCountLogger(this.logger, user.Claims.Count(), null);
                foreach (Claim claim in claims)
                {
                    ClaimLogger(this.logger, claim.Type, claim.Value, null);
                }
            }
            else
            {
                NoClaimsLogger(this.logger, null);
            }

            return Task.FromResult(new AuthenticationState(user));
        }
        catch (Exception ex)
        {
            AuthStateExceptionLogger(this.logger, ex);
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }
    }

    /// <summary>
    ///     Updates the current circuit authentication state with an authenticated user.
    /// </summary>
    /// <param name="principal">Authenticated principal.</param>
    public void SetAuthenticatedUser(ClaimsPrincipal principal)
    {
        this.currentUser = principal;
        this.NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    /// <summary>
    ///     Clears the current circuit authentication state.
    /// </summary>
    public void ClearAuthenticatedUser()
    {
        ClaimsPrincipal anonymous = new(new ClaimsIdentity());
        this.currentUser = anonymous;
        this.NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
    }
}