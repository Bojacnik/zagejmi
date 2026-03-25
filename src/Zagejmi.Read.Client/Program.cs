using System;
using System.Collections.Generic;
using System.Linq;
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
    private readonly AuthTokenService _tokenService;

    public AuthHeaderHandler(AuthTokenService tokenService)
    {
        _tokenService = tokenService;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_tokenService.Token is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenService.Token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}

// This custom AuthenticationStateProvider is the heart of the client-side auth system.
// It tells Blazor about the current user's identity.
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly AuthTokenService _tokenService;

    public CustomAuthenticationStateProvider(AuthTokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = _tokenService.Token;
        var identity = string.IsNullOrEmpty(token)
            ? new ClaimsIdentity() // Not logged in
            : new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");

        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    public void NotifyUserAuthentication(string token)
    {
        _tokenService.Token = token;
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    public void NotifyUserLogout()
    {
        _tokenService.Token = null;
        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);
        var authState = Task.FromResult(new AuthenticationState(user));
        NotifyAuthenticationStateChanged(authState);
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs != null)
        {
            keyValuePairs.TryGetValue("sub", out var id);
            if (id != null) { claims.Add(new Claim(ClaimTypes.NameIdentifier, id.ToString()!)); }

            keyValuePairs.TryGetValue("name", out var name);
            if (name != null) { claims.Add(new Claim(ClaimTypes.Name, name.ToString()!)); }

            keyValuePairs.TryGetValue("email", out var email);
            if (email != null) { claims.Add(new Claim(ClaimTypes.Email, email.ToString()!)); }
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
        builder.Services.AddHttpClient("WriteAPI", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5178");
        });

        // Configure the default HttpClient to use the AuthHeaderHandler and have the correct base address.
        builder.Services.AddScoped(sp => 
        {
            var handler = sp.GetRequiredService<AuthHeaderHandler>();
            handler.InnerHandler = new HttpClientHandler();
            return new HttpClient(handler)
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            };
        });

        await builder.Build().RunAsync();
    }
}
