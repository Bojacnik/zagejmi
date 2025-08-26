using Zagejmi.Client.Services;

namespace Zagejmi.Server.Read.Components;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> LoginAsync(string username, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("api/user/login", new { username, password });

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return result?.Token;
        }

        return null;
    }

    private class LoginResponse
    {
        public string? Token { get; set; }
    }
}
