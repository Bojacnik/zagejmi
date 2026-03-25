using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Zagejmi.Read.Api.Components;

public class AuthService : IAuthService
{
    private readonly HttpClient httpClient;

    public AuthService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

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

    private class LoginResponse
    {
        public string? Token { get; init; }
    }
}