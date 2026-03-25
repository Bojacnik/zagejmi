using System;
using System.Collections.Specialized;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

using LanguageExt;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Zagejmi.Contracts.Failures;
using Zagejmi.Write.Application.CommandHandlers;
using Zagejmi.Write.Application.Commands.User;
using Zagejmi.Write.Infrastructure.Ctx;
using Zagejmi.Write.Infrastructure.Projections;

namespace Zagejmi.Write.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IHandlerRequest<CommandUserCreate, Either<Failure, Guid>> _createUserHandler;
    private readonly ZagejmiContext _dbContext;
    private readonly IHandlerRequest<CommandUserLogin, Either<Failure, string>> _loginUserHandler;

    public UserController(
        IHandlerRequest<CommandUserCreate, Either<Failure, Guid>> createUserHandler,
        IHandlerRequest<CommandUserLogin, Either<Failure, string>> loginUserHandler,
        ZagejmiContext dbContext)
    {
        this._createUserHandler = createUserHandler;
        this._loginUserHandler = loginUserHandler;
        this._dbContext = dbContext;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateUser([FromBody] CommandUserCreate command)
    {
        Either<Failure, Guid> result = await this._createUserHandler.Handle(command, this.HttpContext.RequestAborted);

        return result.Match<IActionResult>(
            guid => this.Ok(guid),
            this.BadRequest);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromForm] string username,
        [FromForm] string password,
        [FromForm] string successUrl,
        [FromForm] string failureUrl)
    {
        CommandUserLogin command = new() { Username = username, Password = password };
        Either<Failure, string> result = await this._loginUserHandler.Handle(command, this.HttpContext.RequestAborted);

        return await result.Match<Task<IActionResult>>(
            async token =>
            {
                JwtSecurityTokenHandler handler = new();
                JwtSecurityToken? jwtToken = handler.ReadJwtToken(token);
                ClaimsIdentity identity = new(jwtToken.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
                ClaimsPrincipal principal = new(identity);

                await this.HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddDays(7)
                    });

                return this.Redirect(successUrl);
            },
            failure =>
            {
                UriBuilder uriBuilder = new(failureUrl);
                NameValueCollection query = HttpUtility.ParseQueryString(uriBuilder.Query);
                query["error"] = failure.Message;
                uriBuilder.Query = query.ToString();
                return Task.FromResult<IActionResult>(this.Redirect(uriBuilder.ToString()));
            });
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout([FromForm] string returnUrl)
    {
        await this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return this.Redirect(returnUrl);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        string? userIdString = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userId))
        {
            return this.Unauthorized();
        }

        UserProjection? user = await this._dbContext.UserProjections
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == userId);

        if (user == null)
        {
            return this.NotFound("User not found.");
        }

        return this.Ok(new { user.Id, user.UserName, user.Email });
    }
}