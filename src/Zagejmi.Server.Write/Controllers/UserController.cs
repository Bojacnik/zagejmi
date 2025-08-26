using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Zagejmi.Server.Application.CommandHandlers;
using Zagejmi.Server.Application.Commands.User;
using Zagejmi.Server.Infrastructure.Ctx;
using Zagejmi.SharedKernel.Failures;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using System.Web;

namespace Zagejmi.Server.Write.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IHandlerRequest<CommandUserCreate, Either<Failure, Guid>> _createUserHandler;
    private readonly IHandlerRequest<CommandUserLogin, Either<Failure, string>> _loginUserHandler;
    private readonly ZagejmiContext _dbContext;

    public UserController(
        IHandlerRequest<CommandUserCreate, Either<Failure, Guid>> createUserHandler,
        IHandlerRequest<CommandUserLogin, Either<Failure, string>> loginUserHandler,
        ZagejmiContext dbContext)
    {
        _createUserHandler = createUserHandler;
        _loginUserHandler = loginUserHandler;
        _dbContext = dbContext;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateUser([FromBody] CommandUserCreate command)
    {
        Either<Failure, Guid> result = await _createUserHandler.Handle(command, HttpContext.RequestAborted);

        return result.Match<IActionResult>(
            Right: guid => Ok(guid),
            Left: BadRequest
        );
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromForm] string Username, [FromForm] string Password, [FromForm] string SuccessUrl, [FromForm] string FailureUrl)
    {
        var command = new CommandUserLogin { Username = Username, Password = Password };
        var result = await _loginUserHandler.Handle(command, HttpContext.RequestAborted);

        return await result.Match<Task<IActionResult>>(
            Right: async token =>
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var identity = new ClaimsIdentity(jwtToken.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddDays(7)
                });

                return Redirect(SuccessUrl);
            },
            Left: failure =>
            {
                var uriBuilder = new UriBuilder(FailureUrl);
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                query["error"] = failure.Message;
                uriBuilder.Query = query.ToString();
                return Task.FromResult<IActionResult>(Redirect(uriBuilder.ToString()));
            }
        );
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout([FromForm] string ReturnUrl)
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect(ReturnUrl);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var user = await _dbContext.UserProjections
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == userId);

        if (user == null)
        {
            return NotFound("User not found.");
        }

        return Ok(new { user.Id, user.UserName, user.Email });
    }
}
