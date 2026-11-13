using System.Collections.Generic;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Zagejmi.Domain.Games;

namespace Zagejmi.Api.Controllers;

/// <summary>
///     The curated list of games and their ranks that services can be offered for.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class GamesController : ControllerBase
{
    /// <summary>
    ///     Lists all games with their ranks (lowest first).
    /// </summary>
    /// <returns>The games.</returns>
    [HttpGet]
    [AllowAnonymous]
    public ActionResult<IReadOnlyList<Game>> List()
    {
        return this.Ok(GameCatalog.All);
    }
}
