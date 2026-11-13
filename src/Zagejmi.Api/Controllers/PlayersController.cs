using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Zagejmi.Api.Extensions;
using Zagejmi.Application.Profiles.SearchPlayers;
using Zagejmi.Shared;

namespace Zagejmi.Api.Controllers;

/// <summary>
///     Public player directory.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly SearchPlayersHandler searchPlayersHandler;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PlayersController" /> class.
    /// </summary>
    /// <param name="searchPlayersHandler">Handler listing players.</param>
    public PlayersController(SearchPlayersHandler searchPlayersHandler)
    {
        this.searchPlayersHandler = searchPlayersHandler;
    }

    /// <summary>
    ///     Lists players, newest first, optionally filtered by display name.
    /// </summary>
    /// <param name="search">Text to look for in the display name.</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Players per page (max 100).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The players on the requested page.</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Search(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 48,
        CancellationToken cancellationToken = default)
    {
        Result<IReadOnlyList<PlayerListItem>> result = await this.searchPlayersHandler.Handle(
            new SearchPlayersQuery { Search = search, Page = page, PageSize = pageSize },
            cancellationToken);

        return result.IsSuccess ? this.Ok(result.Value) : this.Error(result.Error!);
    }
}
