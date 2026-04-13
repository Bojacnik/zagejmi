using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Zagejmi.Read.Client;

// A simple in-memory service to hold the JWT token.
public class AuthTokenService
{
    public string? Token { get; set; }
}

// This handler intercepts outgoing HTTP requests and adds the
// Authorization header if a token is available.
public class AuthHeaderHandler : DelegatingHandler
{
    private readonly AuthTokenService tokenService;

    public AuthHeaderHandler(AuthTokenService tokenService)
    {
        this.tokenService = tokenService;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (this.tokenService.Token is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.tokenService.Token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}

// This custom AuthenticationStateProvider is the heart of the client-side auth system.
// It tells Blazor about the current user's identity.
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly AuthTokenService tokenService;

    public CustomAuthenticationStateProvider(AuthTokenService tokenService)
    {
        this.tokenService = tokenService;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string? token = this.tokenService.Token;
        ClaimsIdentity identity = string.IsNullOrEmpty(token)
            ? new ClaimsIdentity() // Not logged in
            : new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");

        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    public void NotifyUserAuthentication(string token)
    {
        this.tokenService.Token = token;
        ClaimsPrincipal authenticatedUser = new(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
        Task<AuthenticationState> authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        this.NotifyAuthenticationStateChanged(authState);
    }

    public void NotifyUserLogout()
    {
        this.tokenService.Token = null;
        ClaimsIdentity identity = new();
        ClaimsPrincipal user = new(identity);
        Task<AuthenticationState> authState = Task.FromResult(new AuthenticationState(user));
        this.NotifyAuthenticationStateChanged(authState);
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        List<Claim> claims = new();
        string payload = jwt.Split('.')[1];
        byte[] jsonBytes = ParseBase64WithoutPadding(payload);
        Dictionary<string, object>? keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs != null)
        {
            keyValuePairs.TryGetValue("sub", out object? id);
            if (id != null)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, id.ToString()!));
            }

            keyValuePairs.TryGetValue("name", out object? name);
            if (name != null)
            {
                claims.Add(new Claim(ClaimTypes.Name, name.ToString()!));
            }

            keyValuePairs.TryGetValue("email", out object? email);
            if (email != null)
            {
                claims.Add(new Claim(ClaimTypes.Email, email.ToString()!));
            }
        }

        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        return Convert.FromBase64String(base64);
    }
}

internal static class Program
{
    private static async Task Main(string[] args)
    {
        WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

        // Register Blazor's authorization services and our custom provider
        builder.Services.AddAuthorizationCore();
        builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

        // Register the token service as a singleton
        builder.Services.AddSingleton<AuthTokenService>();

        // Register the custom delegating handler
        builder.Services.AddTransient<AuthHeaderHandler>();

        // Configure a named HttpClient for the write-side API
        builder.Services.AddHttpClient(
            "WriteAPI",
            client => { client.BaseAddress = new Uri("http://localhost:5178"); });

        // Configure the default HttpClient to use the AuthHeaderHandler and have the correct base address.
        builder.Services.AddScoped(sp =>
        {
            AuthHeaderHandler handler = sp.GetRequiredService<AuthHeaderHandler>();
            handler.InnerHandler = new HttpClientHandler();
            return new HttpClient(handler)
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            };
        });

        await builder.Build().RunAsync();
    }
}