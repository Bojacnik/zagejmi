using System;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Zagejmi.Write.Infrastructure.Ctx;
using Zagejmi.Write.Infrastructure.Projections;

namespace Zagejmi.Read.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ZagejmiContext _context;

    public ProfileController(ZagejmiContext context)
    {
        this._context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        string? userIdString = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userId))
        {
            return this.Unauthorized();
        }

        PersonProjection? person = await this._context.PersonProjections
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (person == null)
        {
            return this.NotFound("Person not found.");
        }

        return this.Ok(person);
    }
}