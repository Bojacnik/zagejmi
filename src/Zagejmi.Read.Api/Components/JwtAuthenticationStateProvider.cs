using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Zagejmi.Read.Api.Components;

/// <summary>
///     Authentication state provider backed by protected browser storage for JWT mode.
/// </summary>
public sealed class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private const string AccessTokenKey = "auth.access-token";
    private const string RefreshTokenKey = "auth.refresh-token";

    private readonly ProtectedLocalStorage protectedLocalStorage;

    /// <summary>
    ///     Initializes a new instance of the <see cref="JwtAuthenticationStateProvider" /> class.
    /// </summary>
    /// <param name="protectedLocalStorage">Protected local storage accessor.</param>
    public JwtAuthenticationStateProvider(ProtectedLocalStorage protectedLocalStorage)
    {
        this.protectedLocalStorage = protectedLocalStorage;
    }

    /// <inheritdoc />
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string? token = await this.GetAccessTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            return Anonymous();
        }

        ClaimsIdentity identity = CreateIdentity(token);
        if (!identity.IsAuthenticated)
        {
            await this.ClearTokensAsync();
            return Anonymous();
        }

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    /// <summary>
    ///     Persists access and refresh tokens and notifies authentication state change.
    /// </summary>
    /// <param name="accessToken">JWT access token.</param>
    /// <param name="refreshToken">Refresh token.</param>
    /// <returns>A task that completes after tokens are stored and state is refreshed.</returns>
    public async Task SetTokensAsync(string accessToken, string refreshToken)
    {
        await this.protectedLocalStorage.SetAsync(AccessTokenKey, accessToken);
        await this.protectedLocalStorage.SetAsync(RefreshTokenKey, refreshToken);

        ClaimsIdentity identity = CreateIdentity(accessToken);
        this.NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
    }

    /// <summary>
    ///     Removes stored tokens and transitions to anonymous state.
    /// </summary>
    /// <returns>A task that completes after tokens are removed and anonymous state is published.</returns>
    public async Task ClearTokensAsync()
    {
        await this.protectedLocalStorage.DeleteAsync(AccessTokenKey);
        await this.protectedLocalStorage.DeleteAsync(RefreshTokenKey);
        this.NotifyAuthenticationStateChanged(Task.FromResult(Anonymous()));
    }

    /// <summary>
    ///     Reads the current access token from protected storage.
    /// </summary>
    /// <returns>Stored access token when available; otherwise <see langword="null" />.</returns>
    public async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            ProtectedBrowserStorageResult<string> stored =
                await this.protectedLocalStorage.GetAsync<string>(AccessTokenKey);

            return stored.Success ? stored.Value : null;
        }
        catch (InvalidOperationException)
        {
            // During static prerender, JS interop is unavailable.
            return null;
        }
    }

    private static AuthenticationState Anonymous()
    {
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    private static ClaimsIdentity CreateIdentity(string jwtToken)
    {
        try
        {
            JwtSecurityToken token = new JwtSecurityTokenHandler().ReadJwtToken(jwtToken);
            if (token.ValidTo <= DateTime.UtcNow)
            {
                return new ClaimsIdentity();
            }

            List<Claim> claims =
            [
                .. token.Claims,
                new("access_token", jwtToken)
            ];

            string? subject = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            if (!string.IsNullOrWhiteSpace(subject) && claims.All(c => c.Type != ClaimTypes.NameIdentifier))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, subject));
            }

            string? email = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
            if (!string.IsNullOrWhiteSpace(email) && claims.All(c => c.Type != ClaimTypes.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, email));
            }

            string? username = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName)?.Value;
            if (!string.IsNullOrWhiteSpace(username) && claims.All(c => c.Type != ClaimTypes.Name))
            {
                claims.Add(new Claim(ClaimTypes.Name, username));
            }

            return new ClaimsIdentity(claims, "jwt");
        }
        catch
        {
            return new ClaimsIdentity();
        }
    }
}