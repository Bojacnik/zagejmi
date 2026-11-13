using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Domain.Auth;

namespace Zagejmi.Api.Filters;

/// <summary>
///     Enforces ghost mode: a user in ghost mode can only browse, so every state-changing request (anything other than
///     GET/HEAD/OPTIONS) is rejected with 403 unless the endpoint is marked with <see cref="AllowInGhostModeAttribute" />.
/// </summary>
public sealed class GhostModeFilter : IAsyncActionFilter
{
    private static readonly string[] Value =
    [
        "V režimu ducha můžete web pouze prohlížet. Pro tuto akci režim ducha vypněte."
    ];

    private readonly IUserRepository userRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GhostModeFilter" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    public GhostModeFilter(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    /// <inheritdoc />
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        HttpContext http = context.HttpContext;

        bool isReadOnly = HttpMethods.IsGet(http.Request.Method)
                          || HttpMethods.IsHead(http.Request.Method)
                          || HttpMethods.IsOptions(http.Request.Method);

        bool isAllowed = http.GetEndpoint()?.Metadata.GetMetadata<AllowInGhostModeAttribute>() is not null;

        string? userIdClaim = http.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!isReadOnly && !isAllowed && Guid.TryParse(userIdClaim, out Guid userId))
        {
            User? user = await this.userRepository.GetByIdAsync(userId, http.RequestAborted);
            if (user is { IsGhostMode: true })
            {
                context.Result = new ObjectResult(
                    new
                    {
                        code = "User.GhostMode",
                        errors = Value
                    })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };

                return;
            }
        }

        await next();
    }
}