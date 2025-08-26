using Microsoft.AspNetCore.Mvc;

namespace Zagejmi.Server.Read.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AccountController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] LoginModel model, [FromQuery] string? returnUrl)
    {
        if (!ModelState.IsValid)
        {
            return Redirect("/login?error=Invalid%20data");
        }

        HttpClient httpClient = _httpClientFactory.CreateClient("WriteAPI");
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/user/login", model);

        if (response.IsSuccessStatusCode)
        {
            if (!response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? setCookieHeaders))
                return Redirect(returnUrl ?? "/profile");
            
            foreach (string header in setCookieHeaders)
            {
                Response.Headers.Append("Set-Cookie", header);
            }
            return Redirect(returnUrl ?? "/profile");
        }

        string error = await response.Content.ReadAsStringAsync();
        return Redirect($"/login?error={Uri.EscapeDataString(error)}");
    }
}

public class LoginModel
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}
