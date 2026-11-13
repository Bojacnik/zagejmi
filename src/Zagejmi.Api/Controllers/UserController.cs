using System;
using System.Globalization;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Zagejmi.Api.Extensions;
using Zagejmi.Api.Filters;
using Zagejmi.Application.Auth.Users.CreateUser;
using Zagejmi.Application.Auth.Users.DeleteUser;
using Zagejmi.Application.Auth.Users.LoginUser;
using Zagejmi.Application.Auth.Users.SetDefaultProfile;
using Zagejmi.Application.Auth.Users.SetGhostMode;
using Zagejmi.Application.Images;
using Zagejmi.Application.Images.Abstractions;
using Zagejmi.Application.Profiles.AddPlayerService;
using Zagejmi.Application.Profiles.CreatePlayerProfile;
using Zagejmi.Application.Profiles.DeletePlayerService;
using Zagejmi.Application.Profiles.GetUserProfiles;
using Zagejmi.Application.Profiles.SetPlayerBanner;
using Zagejmi.Application.Profiles.SetProfileImage;
using Zagejmi.Application.Profiles.UpdatePlayerService;
using Zagejmi.Domain.Profiles;
using Zagejmi.Shared;

namespace Zagejmi.Api.Controllers;

/// <summary>
///     Represents the controller responsible for handling user-related API requests.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class UserController : ControllerBase
{
    private static readonly ErrorDetail InvalidPrice =
        ErrorDetail.Validation("Service.Price", "Cena musí být celé číslo Goinů, např. 199.");

    private readonly CreateUserHandler createUserHandler;
    private readonly DeleteUserHandler deleteUserHandler;
    private readonly LoginUserHandler loginUserHandler;
    private readonly SetGhostModeHandler setGhostModeHandler;
    private readonly SetDefaultProfileHandler setDefaultProfileHandler;
    private readonly SetProfileImageHandler setProfileImageHandler;
    private readonly SetPlayerBannerHandler setPlayerBannerHandler;
    private readonly AddPlayerServiceHandler addPlayerServiceHandler;
    private readonly UpdatePlayerServiceHandler updatePlayerServiceHandler;
    private readonly DeletePlayerServiceHandler deletePlayerServiceHandler;
    private readonly CreatePlayerProfileHandler createPlayerProfileHandler;
    private readonly GetUserProfilesHandler getUserProfilesHandler;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserController" /> class.
    /// </summary>
    /// <param name="createUserHandler">Handler for registering users.</param>
    /// <param name="deleteUserHandler">Handler for deleting users.</param>
    /// <param name="loginUserHandler">Handler for signing users in.</param>
    /// <param name="setGhostModeHandler">Handler for toggling ghost mode.</param>
    /// <param name="setDefaultProfileHandler">Handler for choosing the default profile.</param>
    /// <param name="setProfileImageHandler">Handler for changing profile pictures.</param>
    /// <param name="setPlayerBannerHandler">Handler for changing the player banner.</param>
    /// <param name="addPlayerServiceHandler">Handler for adding player services.</param>
    /// <param name="updatePlayerServiceHandler">Handler for editing player services.</param>
    /// <param name="deletePlayerServiceHandler">Handler for deleting player services.</param>
    /// <param name="createPlayerProfileHandler">Handler for creating player profiles.</param>
    /// <param name="getUserProfilesHandler">Handler for reading a user's profiles.</param>
    public UserController(
        CreateUserHandler createUserHandler,
        DeleteUserHandler deleteUserHandler,
        LoginUserHandler loginUserHandler,
        SetGhostModeHandler setGhostModeHandler,
        SetDefaultProfileHandler setDefaultProfileHandler,
        SetProfileImageHandler setProfileImageHandler,
        SetPlayerBannerHandler setPlayerBannerHandler,
        AddPlayerServiceHandler addPlayerServiceHandler,
        UpdatePlayerServiceHandler updatePlayerServiceHandler,
        DeletePlayerServiceHandler deletePlayerServiceHandler,
        CreatePlayerProfileHandler createPlayerProfileHandler,
        GetUserProfilesHandler getUserProfilesHandler)
    {
        this.createUserHandler = createUserHandler;
        this.deleteUserHandler = deleteUserHandler;
        this.loginUserHandler = loginUserHandler;
        this.setGhostModeHandler = setGhostModeHandler;
        this.setDefaultProfileHandler = setDefaultProfileHandler;
        this.setProfileImageHandler = setProfileImageHandler;
        this.setPlayerBannerHandler = setPlayerBannerHandler;
        this.addPlayerServiceHandler = addPlayerServiceHandler;
        this.updatePlayerServiceHandler = updatePlayerServiceHandler;
        this.deletePlayerServiceHandler = deletePlayerServiceHandler;
        this.createPlayerProfileHandler = createPlayerProfileHandler;
        this.getUserProfilesHandler = getUserProfilesHandler;
    }

    /// <summary>
    ///     Registers a new user. Every user automatically gets a customer profile.
    /// </summary>
    /// <param name="command">The registration data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The new user ID.</returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command, CancellationToken cancellationToken)
    {
        Result<Guid> result = await this.createUserHandler.Handle(command, cancellationToken);

        return result.IsSuccess ? this.Ok(result.Value) : this.Error(result.Error!);
    }

    /// <summary>
    ///     Signs a user in with their user name or email and password.
    /// </summary>
    /// <param name="command">The sign-in data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The access and refresh tokens.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command, CancellationToken cancellationToken)
    {
        Result<LoginUserResult> result = await this.loginUserHandler.Handle(command, cancellationToken);

        return result.IsSuccess
            ? this.Ok(new { accessToken = result.Value.AccessToken, refreshToken = result.Value.RefreshToken })
            : this.Error(result.Error!);
    }

    /// <summary>
    ///     Deletes the signed-in user's account.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>204 on success.</returns>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        // TODO: Add exception for admins
        if (this.CheckIsCurrentUser(id) is { } denied)
        {
            return denied;
        }

        await this.deleteUserHandler.Handle(new DeleteUserCommand { Id = id }, cancellationToken);

        return this.NoContent();
    }

    /// <summary>
    ///     Returns the customer and player profiles of the signed-in user.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user's profiles.</returns>
    [HttpGet("{id:guid}/profiles")]
    [Authorize]
    public async Task<IActionResult> GetProfiles(Guid id, CancellationToken cancellationToken)
    {
        if (this.CheckIsCurrentUser(id) is { } denied)
        {
            return denied;
        }

        Result<UserProfilesView> result = await this.getUserProfilesHandler.Handle(id, cancellationToken);

        return result.IsSuccess ? this.Ok(result.Value) : this.Error(result.Error!);
    }

    /// <summary>
    ///     Creates the player profile (Hráč/Hráčka) of the signed-in user.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="request">The profile data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The new profile ID.</returns>
    [HttpPost("{id:guid}/profiles/player")]
    [Authorize]
    public async Task<IActionResult> CreatePlayerProfile(
        Guid id,
        [FromBody] CreatePlayerProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (this.CheckIsCurrentUser(id) is { } denied)
        {
            return denied;
        }

        Result<Guid> result = await this.createPlayerProfileHandler.Handle(
            new CreatePlayerProfileCommand
            {
                UserId = id,
                DisplayName = request.DisplayName,
                Description = request.Description,
            },
            cancellationToken);

        return result.IsSuccess ? this.Ok(result.Value) : this.Error(result.Error!);
    }

    /// <summary>
    ///     Turns ghost mode on or off for the signed-in user. Allowed in ghost mode, otherwise a ghost could never leave it.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="request">Whether ghost mode should be enabled.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>204 on success.</returns>
    [HttpPut("{id:guid}/ghost-mode")]
    [Authorize]
    [AllowInGhostMode]
    public async Task<IActionResult> SetGhostMode(
        Guid id,
        [FromBody] SetGhostModeRequest request,
        CancellationToken cancellationToken)
    {
        if (this.CheckIsCurrentUser(id) is { } denied)
        {
            return denied;
        }

        Result result = await this.setGhostModeHandler.Handle(
            new SetGhostModeCommand { UserId = id, Enabled = request.Enabled },
            cancellationToken);

        return result.IsSuccess ? this.NoContent() : this.Error(result.Error!);
    }

    /// <summary>
    ///     Sets which profile becomes active right after the signed-in user logs in.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="request">The default profile.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>204 on success.</returns>
    [HttpPut("{id:guid}/default-profile")]
    [Authorize]
    public async Task<IActionResult> SetDefaultProfile(
        Guid id,
        [FromBody] SetDefaultProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (this.CheckIsCurrentUser(id) is { } denied)
        {
            return denied;
        }

        Result result = await this.setDefaultProfileHandler.Handle(
            new SetDefaultProfileCommand { UserId = id, Profile = request.Profile },
            cancellationToken);

        return result.IsSuccess ? this.NoContent() : this.Error(result.Error!);
    }

    /// <summary>
    ///     Changes the profile picture of the signed-in user's customer or player profile.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="profile">Which profile: <c>Customer</c> or <c>Player</c>.</param>
    /// <param name="file">The image (JPG, PNG or WebP, max 10 MB).</param>
    /// <param name="crop">Optional crop as fractions of the image; without it the center square is used.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The new image ID.</returns>
    [HttpPut("{id:guid}/profiles/{profile}/image")]
    [Authorize]
    [RequestSizeLimit(ImageUploader.MaxUploadBytes + (64 * 1024))]
    public async Task<IActionResult> SetProfileImage(
        Guid id,
        ProfileType profile,
        IFormFile file,
        [FromForm] CropForm crop,
        CancellationToken cancellationToken)
    {
        if (this.CheckIsCurrentUser(id) is { } denied)
        {
            return denied;
        }

        await using Stream content = file.OpenReadStream();
        Result<Guid> result = await this.setProfileImageHandler.Handle(
            new SetProfileImageCommand { UserId = id, Profile = profile, Content = content, Crop = crop.ToCropRegion() },
            cancellationToken);

        return result.IsSuccess ? this.Ok(result.Value) : this.Error(result.Error!);
    }

    /// <summary>
    ///     Changes the banner of the signed-in user's player profile.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="file">The image (JPG, PNG or WebP, max 10 MB).</param>
    /// <param name="crop">Optional crop as fractions of the image; without it the center 4:1 strip is used.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The new image ID.</returns>
    [HttpPut("{id:guid}/profiles/player/banner")]
    [Authorize]
    [RequestSizeLimit(ImageUploader.MaxUploadBytes + (64 * 1024))]
    public async Task<IActionResult> SetPlayerBanner(
        Guid id,
        IFormFile file,
        [FromForm] CropForm crop,
        CancellationToken cancellationToken)
    {
        if (this.CheckIsCurrentUser(id) is { } denied)
        {
            return denied;
        }

        await using Stream content = file.OpenReadStream();
        Result<Guid> result = await this.setPlayerBannerHandler.Handle(
            new SetPlayerBannerCommand { UserId = id, Content = content, Crop = crop.ToCropRegion() },
            cancellationToken);

        return result.IsSuccess ? this.Ok(result.Value) : this.Error(result.Error!);
    }

    /// <summary>
    ///     Adds a service to the signed-in user's player profile.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="request">The service (multipart form; the image is optional).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The new service ID.</returns>
    [HttpPost("{id:guid}/profiles/player/services")]
    [Authorize]
    [RequestSizeLimit(ImageUploader.MaxUploadBytes + (64 * 1024))]
    public async Task<IActionResult> AddPlayerService(
        Guid id,
        [FromForm] AddPlayerServiceRequest request,
        CancellationToken cancellationToken)
    {
        if (this.CheckIsCurrentUser(id) is { } denied)
        {
            return denied;
        }

        if (!TryParsePrice(request.Price, out long price))
        {
            return this.Error(InvalidPrice);
        }

        await using Stream? image = request.Image?.OpenReadStream();
        Result<Guid> result = await this.addPlayerServiceHandler.Handle(
            new AddPlayerServiceCommand
            {
                UserId = id,
                Name = request.Name,
                Description = request.Description,
                Category = request.Category,
                Price = price,
                PriceUnit = request.PriceUnit,
                Image = image,
            },
            cancellationToken);

        return result.IsSuccess ? this.Ok(result.Value) : this.Error(result.Error!);
    }

    /// <summary>
    ///     Edits a service of the signed-in user's player profile.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="serviceId">The service ID.</param>
    /// <param name="request">The service (multipart form). Send a new image to replace it, or RemoveImage=true.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>204 on success.</returns>
    [HttpPut("{id:guid}/profiles/player/services/{serviceId:guid}")]
    [Authorize]
    [RequestSizeLimit(ImageUploader.MaxUploadBytes + (64 * 1024))]
    public async Task<IActionResult> UpdatePlayerService(
        Guid id,
        Guid serviceId,
        [FromForm] UpdatePlayerServiceRequest request,
        CancellationToken cancellationToken)
    {
        if (this.CheckIsCurrentUser(id) is { } denied)
        {
            return denied;
        }

        if (!TryParsePrice(request.Price, out long price))
        {
            return this.Error(InvalidPrice);
        }

        await using Stream? image = request.Image?.OpenReadStream();
        Result result = await this.updatePlayerServiceHandler.Handle(
            new UpdatePlayerServiceCommand
            {
                UserId = id,
                ServiceId = serviceId,
                Name = request.Name,
                Description = request.Description,
                Category = request.Category,
                Price = price,
                PriceUnit = request.PriceUnit,
                NewImage = image,
                RemoveImage = request.RemoveImage,
            },
            cancellationToken);

        return result.IsSuccess ? this.NoContent() : this.Error(result.Error!);
    }

    /// <summary>
    ///     Deletes a service of the signed-in user's player profile.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="serviceId">The service ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>204 on success.</returns>
    [HttpDelete("{id:guid}/profiles/player/services/{serviceId:guid}")]
    [Authorize]
    public async Task<IActionResult> DeletePlayerService(Guid id, Guid serviceId, CancellationToken cancellationToken)
    {
        if (this.CheckIsCurrentUser(id) is { } denied)
        {
            return denied;
        }

        Result result = await this.deletePlayerServiceHandler.Handle(
            new DeletePlayerServiceCommand { UserId = id, ServiceId = serviceId },
            cancellationToken);

        return result.IsSuccess ? this.NoContent() : this.Error(result.Error!);
    }

    /// <summary>
    ///     Parses a price sent by an API client: a whole number of Goins (e.g. "199"). An empty value means 0.
    /// </summary>
    private static bool TryParsePrice(string? value, out long price)
    {
        price = 0;
        return string.IsNullOrWhiteSpace(value)
               || long.TryParse(value.Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out price);
    }

    /// <summary>
    ///     Returns an error result when <paramref name="id" /> is not the signed-in user; otherwise <see langword="null" />.
    /// </summary>
    private IActionResult? CheckIsCurrentUser(Guid id)
    {
        string? userIdClaim = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out Guid authenticatedUserId))
        {
            return this.Unauthorized();
        }

        return authenticatedUserId == id ? null : this.Forbid();
    }

    /// <summary>
    ///     Request body for <see cref="CreatePlayerProfile" />.
    /// </summary>
    /// <param name="DisplayName">The public display name.</param>
    /// <param name="Description">The optional profile description.</param>
    public sealed record CreatePlayerProfileRequest(string DisplayName, string? Description);

    /// <summary>
    ///     Request body for <see cref="SetGhostMode" />.
    /// </summary>
    /// <param name="Enabled">Whether ghost mode should be enabled.</param>
    public sealed record SetGhostModeRequest(bool Enabled);

    /// <summary>
    ///     Body of the set-default-profile request.
    /// </summary>
    /// <param name="Profile">The profile to activate after sign-in.</param>
    public sealed record SetDefaultProfileRequest(ProfileType Profile);

    /// <summary>
    ///     Optional crop sent with an image upload: <c>cropX</c>, <c>cropY</c>, <c>cropWidth</c>, <c>cropHeight</c> as
    ///     fractions (0–1, dot as decimal separator) of the correctly rotated image.
    /// </summary>
    public sealed class CropForm
    {
        /// <summary>
        ///     Gets or sets the left edge.
        /// </summary>
        public string? CropX { get; set; }

        /// <summary>
        ///     Gets or sets the top edge.
        /// </summary>
        public string? CropY { get; set; }

        /// <summary>
        ///     Gets or sets the width.
        /// </summary>
        public string? CropWidth { get; set; }

        /// <summary>
        ///     Gets or sets the height.
        /// </summary>
        public string? CropHeight { get; set; }

        /// <summary>
        ///     Returns the crop, or <see langword="null" /> when any value is missing or not a number.
        /// </summary>
        /// <returns>The crop region.</returns>
        public CropRegion? ToCropRegion()
        {
            return TryParse(this.CropX, out double x) && TryParse(this.CropY, out double y)
                   && TryParse(this.CropWidth, out double width) && TryParse(this.CropHeight, out double height)
                ? new CropRegion(x, y, width, height)
                : null;
        }

        private static bool TryParse(string? value, out double result)
        {
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }
    }

    /// <summary>
    ///     Multipart form of the edit-service request.
    /// </summary>
    public sealed class UpdatePlayerServiceRequest
    {
        /// <summary>
        ///     Gets or sets the service name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets what the service includes.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        ///     Gets or sets an optional category.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        ///     Gets or sets the price in whole Goins (e.g. "199").
        /// </summary>
        public string? Price { get; set; }

        /// <summary>
        ///     Gets or sets what the price is for, e.g. "/ hodina".
        /// </summary>
        public string? PriceUnit { get; set; }

        /// <summary>
        ///     Gets or sets a new image, or <see langword="null" /> to keep the current one.
        /// </summary>
        public IFormFile? Image { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to remove the current image.
        /// </summary>
        public bool RemoveImage { get; set; }
    }

    /// <summary>
    ///     Multipart form of the add-service request.
    /// </summary>
    public sealed class AddPlayerServiceRequest
    {
        /// <summary>
        ///     Gets or sets the service name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets what the service includes.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        ///     Gets or sets an optional category.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        ///     Gets or sets the price in whole Goins (e.g. "199").
        /// </summary>
        public string? Price { get; set; }

        /// <summary>
        ///     Gets or sets what the price is for, e.g. "/ hodina".
        /// </summary>
        public string? PriceUnit { get; set; }

        /// <summary>
        ///     Gets or sets an optional image.
        /// </summary>
        public IFormFile? Image { get; set; }
    }
}
