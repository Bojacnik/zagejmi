using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Zagejmi.Api.Extensions;
using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Profiles;
using Zagejmi.Application.Profiles.SearchServices;
using Zagejmi.Domain.Games;
using Zagejmi.Shared;

namespace Zagejmi.Api.Controllers;

/// <summary>
///     Public service catalog (the Služby page).
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly SearchServicesHandler searchServicesHandler;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ServicesController" /> class.
    /// </summary>
    /// <param name="searchServicesHandler">Handler listing services.</param>
    public ServicesController(SearchServicesHandler searchServicesHandler)
    {
        this.searchServicesHandler = searchServicesHandler;
    }

    /// <summary>
    ///     Lists available services of all players, most complete listings first (images), then newest.
    /// </summary>
    /// <param name="search">Text to look for in the service name or the player's display name.</param>
    /// <param name="game">A game key from <c>GET api/v1/games</c>.</param>
    /// <param name="minRank">The lowest rank key to include (requires <paramref name="game" />).</param>
    /// <param name="playStyles">"Competitive", "Casual" or "Both"; services offering any of them match.</param>
    /// <param name="gender">"Women", "Men" or "Others" for one group of the gender split; omit for everyone.</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Services per page (max 100).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The services on the requested page and how many match in total.</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Search(
        [FromQuery] string? search,
        [FromQuery] string? game,
        [FromQuery] string? minRank,
        [FromQuery] PlayStyles playStyles = PlayStyles.None,
        [FromQuery] GenderGroup? gender = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 48,
        CancellationToken cancellationToken = default)
    {
        Result<PagedResult<ServiceListItem>> result = await this.searchServicesHandler.Handle(
            new SearchServicesQuery
            {
                Search = search,
                GameKey = game,
                MinRankKey = minRank,
                PlayStyles = playStyles,
                Gender = gender,
                Page = page,
                PageSize = pageSize,
            },
            cancellationToken);

        return result.IsSuccess ? this.Ok(result.Value) : this.Error(result.Error!);
    }
}
