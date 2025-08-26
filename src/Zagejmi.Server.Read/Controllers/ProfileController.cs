using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using Zagejmi.Server.Infrastructure.Ctx;

namespace Zagejmi.Server.Read.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly ZagejmiContext _context;

        public ProfileController(ZagejmiContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var person = await _context.PersonProjections
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (person == null)
            {
                return NotFound("Person not found.");
            }

            return Ok(person);
        }
    }
}
