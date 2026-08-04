using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Zagejmi.Api.Services;
using Zagejmi.Infrastructure.Ctx;

namespace Zagejmi.Api.Controllers;

/// <summary>
///     Read-side endpoints for querying Associate profile projections.
///     Provides public access to Associate profile information and their services.
/// </summary>
[ApiController]
[Route("api/v1/read/associates")]
public sealed class AssociatesController : ControllerBase
{
    private readonly ZagejmiContext context;
    private readonly ReadOnlyProfileImageStorage profileImageStorage;
    private readonly ReadOnlyServiceImageStorage serviceImageStorage;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AssociatesController" /> class.
    /// </summary>
    /// <param name="context">Read database context.</param>
    /// <param name="profileImageStorage">Profile image storage service.</param>
    /// <param name="serviceImageStorage">Service image storage service.</param>
    public AssociatesController(
        ZagejmiContext context,
        ReadOnlyProfileImageStorage profileImageStorage,
        ReadOnlyServiceImageStorage serviceImageStorage)
    {
        this.context = context;
        this.profileImageStorage = profileImageStorage;
        this.serviceImageStorage = serviceImageStorage;
    }

    /// <summary>
    ///     Gets a public Associate profile with all their services by profile ID.
    /// </summary>
    /// <param name="profileId">The Associate profile ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The Associate profile DTO with services, or 404 if not found.</returns>
    [HttpGet("{profileId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAssociateProfile(
        [FromRoute] Guid profileId,
        CancellationToken cancellationToken)
    {
        // Query the Associate profile with related services
        var profileData = await context.AssociateProfiles
            .AsNoTracking()
            .Where(p => p.ProfileId == profileId)
            .Select(p => new
            {
                p.ProfileId,
                p.UserId,
                p.DisplayName,
                p.ProfileImageBlobKey,
                p.BannerImageBlobKey,
                p.Bio,
                p.Reputation,
                p.Level,
                p.CompletedSessions,
                p.CreatedAtUtc,
                p.IsOnline,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (profileData == null) return NotFound();

        // Query services separately to avoid loading entire related collections
        var services = await context.Services
            .AsNoTracking()
            .Where(s => s.ProfileId == profileId)
            .OrderBy(s => s.Name)
            .Select(s => new
            {
                s.ServiceId,
                s.ProfileId,
                s.Name,
                s.Description,
                s.Category,
                s.Rating,
                s.Served,
                s.Price,
                s.PriceSuffix,
                s.PromoTag,
                s.ImageBlobKey,
                s.IsAvailable,
                s.ResponseTime,
            })
            .ToListAsync(cancellationToken);

        // Map to DTOs in-memory to avoid EF expression translation issues
        List<ServiceDto> servicesDtos = services.Select(s => new ServiceDto
        {
            ServiceId = s.ServiceId,
            ProfileId = s.ProfileId,
            Name = s.Name,
            Description = s.Description,
            Category = s.Category,
            Rating = s.Rating,
            Served = s.Served,
            Price = s.Price,
            PriceSuffix = s.PriceSuffix,
            PromoTag = s.PromoTag,
            ImageUrl = string.IsNullOrWhiteSpace(s.ImageBlobKey)
                ? null
                : BuildServiceImageUrl(s.ImageBlobKey),
            IsAvailable = s.IsAvailable,
            ResponseTime = s.ResponseTime,
        }).ToList();

        AssociateProfileDto profileDto = new()
        {
            ProfileId = profileData.ProfileId,
            UserId = profileData.UserId,
            DisplayName = profileData.DisplayName,
            ProfileImageUrl = string.IsNullOrWhiteSpace(profileData.ProfileImageBlobKey)
                ? null
                : BuildProfileImageUrl(profileData.ProfileImageBlobKey),
            BannerImageUrl = string.IsNullOrWhiteSpace(profileData.BannerImageBlobKey)
                ? null
                : BuildBannerImageUrl(profileData.BannerImageBlobKey),
            Bio = profileData.Bio,
            Reputation = profileData.Reputation,
            Level = profileData.Level,
            CompletedSessions = profileData.CompletedSessions,
            CreatedAt = profileData.CreatedAtUtc,
            IsOnline = profileData.IsOnline,
            Services = servicesDtos,
        };

        return this.Ok(profileDto);
    }

    /// <summary>
    ///     Gets a banner image for an Associate profile by blob key.
    /// </summary>
    /// <param name="blobKey">The banner image blob key.</param>
    /// <returns>Image content when found; otherwise 404 Not Found.</returns>
    [HttpGet("banner-images/{blobKey}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBannerImage([FromRoute] string blobKey)
    {
        // Note: For now, banner images are served from the same location as profile images.
        // In a production system, you may want separate storage or a dedicated banner image service.
        (byte[] Content, string ContentType)? image = await profileImageStorage.ReadProfileImageAsync(
            blobKey,
            HttpContext.RequestAborted);

        if (image == null) return NotFound();

        return File(image.Value.Content, image.Value.ContentType);
    }

    /// <summary>
    ///     Gets a service image by blob key.
    /// </summary>
    /// <param name="blobKey">The service image blob key.</param>
    /// <returns>Image content when found; otherwise 404 Not Found.</returns>
    [HttpGet("service-images/{blobKey}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetServiceImage([FromRoute] string blobKey)
    {
        (byte[] Content, string ContentType)? image = await serviceImageStorage.ReadServiceImageAsync(
            blobKey,
            HttpContext.RequestAborted);

        if (image == null) return NotFound();

        return File(image.Value.Content, image.Value.ContentType);
    }

    /// <summary>
    ///     Searches for Associate profiles by display name with pagination.
    /// </summary>
    /// <param name="search">The search query string.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of results per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of matching Associate profiles with pagination info.</returns>
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> SearchAssociates(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // Validate pagination parameters
        if (page < 1) page = 1;

        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        // Query profiles matching the search term
        // Note: EF Core LIKE operator is case-insensitive on PostgreSQL by default
        // When search is empty, return all profiles
        IQueryable<AssociateProfileProjection> query = context.AssociateProfiles
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p =>
                EF.Functions.Like(p.DisplayName, $"%{search}%") ||
                (p.Bio != null && EF.Functions.Like(p.Bio, $"%{search}%")));

        int totalCount = await query.CountAsync(cancellationToken);

        if (totalCount == 0)
        {
            await this.associateProjectionRepairService.EnsureAssociatesProjectedAsync(cancellationToken);
            totalCount = await query.CountAsync(cancellationToken);
        }

        var profiles = await query
            .OrderByDescending(p => p.Reputation)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.ProfileId,
                p.UserId,
                p.DisplayName,
                p.ProfileImageBlobKey,
                p.Reputation,
                p.Level,
                p.IsOnline,
            })
            .ToListAsync(cancellationToken);

        var dtos = profiles.Select(p => new
        {
            p.ProfileId,
            p.DisplayName,
            ProfileImageUrl = string.IsNullOrWhiteSpace(p.ProfileImageBlobKey)
                ? null
                : BuildProfileImageUrl(p.ProfileImageBlobKey),
            p.Reputation,
            p.Level,
            p.IsOnline,
        }).ToList();

        return Ok(
            new
            {
                Data = dtos,
                Total = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            });
    }

    /// <summary>
    ///     Gets diagnostic information about the Associate profiles in the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Diagnostic info showing total count and sample data.</returns>
    [HttpGet("diagnostics")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDiagnostics(CancellationToken cancellationToken)
    {
        try
        {
            int totalAssociates = await context.AssociateProfiles.CountAsync(cancellationToken);
            int totalServices = await context.Services.CountAsync(cancellationToken);

            var sampleAssociates = await context.AssociateProfiles
                .AsNoTracking()
                .Take(5)
                .Select(p => new
                {
                    p.ProfileId,
                    p.DisplayName,
                    p.Reputation,
                    ServiceCount = p.Services.Count,
                })
                .ToListAsync(cancellationToken);

            return Ok(
                new
                {
                    TotalAssociates = totalAssociates,
                    TotalServices = totalServices,
                    SampleAssociates = sampleAssociates,
                    Message = "Database diagnostics",
                });
        }
        catch (Exception ex)
        {
            return StatusCode(
                500,
                new { Error = "Diagnostics failed", Exception = ex.Message, ex.StackTrace });
        }
    }

    /// <summary>
    ///     Gets the highlight associates for the home page pedestal.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Most requested and most starred associate profiles.</returns>
    [HttpGet("highlights")]
    [AllowAnonymous]
    public async Task<IActionResult> GetHighlights(CancellationToken cancellationToken)
    {
        await this.associateProjectionRepairService.EnsureAssociatesProjectedAsync(cancellationToken);

        var mostRequested = await context.AssociateProfiles
            .AsNoTracking()
            .OrderByDescending(p => p.CompletedSessions)
            .ThenByDescending(p => p.Reputation)
            .Select(p => new
            {
                p.ProfileId,
                p.DisplayName,
                p.ProfileImageBlobKey,
                p.Reputation,
                p.CompletedSessions,
                p.IsOnline,
            })
            .FirstOrDefaultAsync(cancellationToken);

        var mostStarred = await context.AssociateProfiles
            .AsNoTracking()
            .OrderByDescending(p => p.Reputation)
            .ThenByDescending(p => p.CompletedSessions)
            .Select(p => new
            {
                p.ProfileId,
                p.DisplayName,
                p.ProfileImageBlobKey,
                p.Reputation,
                p.CompletedSessions,
                p.IsOnline,
            })
            .FirstOrDefaultAsync(cancellationToken);

        var response = new
        {
            MostRequested = mostRequested == null
                ? null
                : new
                {
                    mostRequested.ProfileId,
                    mostRequested.DisplayName,
                    ProfileImageUrl = string.IsNullOrWhiteSpace(mostRequested.ProfileImageBlobKey)
                        ? null
                        : BuildProfileImageUrl(mostRequested.ProfileImageBlobKey),
                    mostRequested.Reputation,
                    mostRequested.CompletedSessions,
                    mostRequested.IsOnline,
                },
            MostStarred = mostStarred == null
                ? null
                : new
                {
                    mostStarred.ProfileId,
                    mostStarred.DisplayName,
                    ProfileImageUrl = string.IsNullOrWhiteSpace(mostStarred.ProfileImageBlobKey)
                        ? null
                        : BuildProfileImageUrl(mostStarred.ProfileImageBlobKey),
                    mostStarred.Reputation,
                    mostStarred.CompletedSessions,
                    mostStarred.IsOnline,
                },
        };

        return Ok(response);
    }

    /// <summary>
    ///     Builds the full URL for a profile image by blob key.
    /// </summary>
    /// <param name="blobKey">The image blob key.</param>
    /// <returns>Full URL to the profile image endpoint.</returns>
    private string BuildProfileImageUrl(string blobKey)
    {
        string escapedBlobKey = Uri.EscapeDataString(blobKey);
        return $"{Request.Scheme}://{Request.Host}/api/v1/read/users/profile-images/{escapedBlobKey}";
    }

    /// <summary>
    ///     Builds the full URL for a banner image by blob key.
    /// </summary>
    /// <param name="blobKey">The banner image blob key.</param>
    /// <returns>Full URL to the banner image endpoint.</returns>
    private string BuildBannerImageUrl(string blobKey)
    {
        string escapedBlobKey = Uri.EscapeDataString(blobKey);
        return $"{Request.Scheme}://{Request.Host}/api/v1/read/associates/banner-images/{escapedBlobKey}";
    }

    /// <summary>
    ///     Builds the full URL for a service image by blob key.
    /// </summary>
    /// <param name="blobKey">The service image blob key.</param>
    /// <returns>Full URL to the service image endpoint.</returns>
    private string BuildServiceImageUrl(string blobKey)
    {
        string escapedBlobKey = Uri.EscapeDataString(blobKey);
        return $"{Request.Scheme}://{Request.Host}/api/v1/read/associates/service-images/{escapedBlobKey}";
    }
}