using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace Zagejmi.Read.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    private readonly IHttpClientFactory httpClientFactory;

    public AccountController(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] LoginModel model, [FromQuery] string? returnUrl)
    {
        if (!this.ModelState.IsValid)
        {
            return this.Redirect("/login?error=Invalid%20data");
        }

        HttpClient httpClient = this.httpClientFactory.CreateClient("WriteAPI");
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/user/login", model);

        if (response.IsSuccessStatusCode)
        {
            if (!response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? setCookieHeaders))
            {
                return this.Redirect(returnUrl ?? "/profile");
            }

            foreach (string header in setCookieHeaders)
            {
                this.Response.Headers.Append("Set-Cookie", header);
            }

            return this.Redirect(returnUrl ?? "/profile");
        }

        string error = await response.Content.ReadAsStringAsync();
        return this.Redirect($"/login?error={Uri.EscapeDataString(error)}");
    }
}

public class LoginModel
{
    public string Username { get; set; } = "";

    public string Password { get; set; } = "";
}