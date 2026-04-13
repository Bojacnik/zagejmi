using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Zagejmi.Contracts.Dtos;
using Zagejmi.Read.Api.Ctx;

namespace Zagejmi.Read.Api.Controllers;

/// <summary>
///     Read-side endpoints for querying user profile projections.
/// </summary>
[ApiController]
[Route("api/v1/read/users")]
public sealed class UserProfilesController : ControllerBase
{
    private readonly IConfiguration configuration;
    private readonly ZagejmiReadContext readContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserProfilesController" /> class.
    /// </summary>
    /// <param name="readContext">Read database context.</param>
    /// <param name="configuration">Application configuration.</param>
    public UserProfilesController(ZagejmiReadContext readContext, IConfiguration configuration)
    {
        this.readContext = readContext;
        this.configuration = configuration;
    }

    /// <summary>
    ///     Gets profile projections for the specified user.
    /// </summary>
    /// <param name="userId">User identifier from route.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of profile DTOs for the user.</returns>
    [HttpGet("{userId:guid}/profiles")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetProfiles([FromRoute] Guid userId, CancellationToken cancellationToken)
    {
        if (!this.IsSubjectMatchingUserId(userId))
        {
            return this.Forbid();
        }

        string? writeBaseAddress = this.configuration["WriteApi:BaseAddress"];

        List<UserProfileDto> profiles = await this.readContext.UserProfiles
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.UpdatedAtUtc)
            .Select(x => new UserProfileDto
            {
                ProfileId = x.ProfileId,
                UserId = x.UserId,
                ProfileType = x.ProfileType,
                DisplayName = x.DisplayName,
                ProfileImageUrl = string.IsNullOrWhiteSpace(x.ProfileImageBlobKey)
                    ? null
                    : BuildProfileImageUrl(writeBaseAddress, x.ProfileImageBlobKey)
            })
            .ToListAsync(cancellationToken);

        return this.Ok(profiles);
    }

    private static string BuildProfileImageUrl(string? writeBaseAddress, string blobKey)
    {
        string escapedBlobKey = Uri.EscapeDataString(blobKey);

        if (string.IsNullOrWhiteSpace(writeBaseAddress))
        {
            return $"/api/v1/users/profile-images/{escapedBlobKey}";
        }

        Uri baseUri = new(writeBaseAddress.EndsWith('/') ? writeBaseAddress : writeBaseAddress + "/");
        return new Uri(baseUri, $"api/v1/users/profile-images/{escapedBlobKey}").ToString();
    }

    private bool IsSubjectMatchingUserId(Guid userId)
    {
        string? subjectValue = this.User.FindFirstValue(ClaimTypes.NameIdentifier)
                               ?? this.User.FindFirstValue("sub");

        return Guid.TryParse(subjectValue, out Guid subjectId) && subjectId == userId;
    }
}