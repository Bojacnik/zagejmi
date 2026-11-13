using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Zagejmi.Api.Components;

/// <summary>
///     HTTP-based authentication service used by the Blazor UI.
/// </summary>
public class AuthService : IAuthService
{
    private readonly HttpClient httpClient;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthService" /> class.
    /// </summary>
    /// <param name="httpClient">HTTP client for API calls.</param>
    public AuthService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    /// <summary>
    ///     Attempts to authenticate and returns an access token when successful.
    /// </summary>
    /// <param name="username">User name.</param>
    /// <param name="password">Plain text password.</param>
    /// <returns>JWT token if login succeeds; otherwise <see langword="null" />.</returns>
    public async Task<string?> LoginAsync(string username, string password)
    {
        HttpResponseMessage response = await this.httpClient.PostAsJsonAsync(
            "api/user/login",
            new { username, password });

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        LoginResponse? result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return result?.Token;
    }

    private sealed class LoginResponse
    {
        public string? Token { get; init; }
    }
}