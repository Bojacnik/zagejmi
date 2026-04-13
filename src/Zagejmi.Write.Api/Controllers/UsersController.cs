using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

using LanguageExt;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Zagejmi.Contracts.Commands;
using Zagejmi.Contracts.Dtos;
using Zagejmi.Shared.Failures;
using Zagejmi.Shared.Models;
using Zagejmi.Write.Application.Abstractions;
using Zagejmi.Write.Domain.Profile.Events;
using Zagejmi.Write.Infrastructure;
using Zagejmi.Write.Infrastructure.Ctx;

namespace Zagejmi.Write.Api.Controllers;

/// <summary>
///     Handles user management operations.
///     Provides endpoints for user registration and account creation.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : ControllerBase
{
    private static readonly Action<ILogger, string, Exception?> VerificationEmailFailedLogger =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            default,
            "[UsersController] User created but verification email failed: {Reason}");

    private readonly IHandlerRequest<UserCreateProfileCommand, Either<Failure, Guid>> createProfileHandler;

    /// <summary>
    ///     The handler for processing user creation commands.
    /// </summary>
    private readonly IHandlerRequest<UserCreateCommand, Either<Failure, Guid>> createUserHandler;

    private readonly ILogger<UsersController> logger;
    private readonly IProfileImageStorage profileImageStorage;

    private readonly IHandlerRequest<UserSendEmailVerificationCommand, Either<Failure, Unit>>
        sendEmailVerificationHandler;

    private readonly IHandlerRequest<UserUpdateProfileImageCommand, Either<Failure, Unit>> updateProfileImageHandler;

    private readonly ZagejmiWriteContext writeContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UsersController" /> class.
    /// </summary>
    /// <param name="createUserHandler">The handler for processing create user commands.</param>
    /// <param name="createProfileHandler">The handler for creating user profiles.</param>
    /// <param name="updateProfileImageHandler">The handler for updating profile images.</param>
    /// <param name="sendEmailVerificationHandler">The handler for sending verification emails.</param>
    /// <param name="profileImageStorage">Storage service for reading profile images.</param>
    /// <param name="writeContext">Write database context used for event-backed profile projection reads.</param>
    /// <param name="logger">The logger instance.</param>
    public UsersController(
        IHandlerRequest<UserCreateCommand, Either<Failure, Guid>> createUserHandler,
        IHandlerRequest<UserCreateProfileCommand, Either<Failure, Guid>> createProfileHandler,
        IHandlerRequest<UserUpdateProfileImageCommand, Either<Failure, Unit>> updateProfileImageHandler,
        IHandlerRequest<UserSendEmailVerificationCommand, Either<Failure, Unit>> sendEmailVerificationHandler,
        IProfileImageStorage profileImageStorage,
        ZagejmiWriteContext writeContext,
        ILogger<UsersController> logger)
    {
        this.createUserHandler = createUserHandler;
        this.createProfileHandler = createProfileHandler;
        this.updateProfileImageHandler = updateProfileImageHandler;
        this.sendEmailVerificationHandler = sendEmailVerificationHandler;
        this.profileImageStorage = profileImageStorage;
        this.writeContext = writeContext;
        this.logger = logger;
    }

    /// <summary>
    ///     Creates a new user account with the provided credentials.
    /// </summary>
    /// <param name="command">The create user command containing username, password, and email.</param>
    /// <returns>
    ///     A 201 Created response with the newly created user's ID on success,
    ///     or a 400 BadRequest response with an error message on failure.
    /// </returns>
    /// <remarks>
    ///     This endpoint validates:
    ///     - Username is at least 3 characters and at most 50 characters
    ///     - Password meets strength requirements (8+ chars, uppercase, lowercase, digit, special char)
    ///     - Email is in valid format and at most 254 characters
    ///     The password is securely hashed using Argon2 before storage.
    ///     A domain event is raised to initiate event-driven workflows.
    /// </remarks>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateUser([FromBody] UserCreateCommand command)
    {
        Either<Failure, Guid> result = await this.createUserHandler.Handle(command, this.HttpContext.RequestAborted);

        return await result.Match<Task<IActionResult>>(
            async userId =>
            {
                string message = "User created. Check your email to verify ownership.";

                try
                {
                    Either<Failure, Unit> verificationResult = await this.sendEmailVerificationHandler.Handle(
                        new UserSendEmailVerificationCommand { UserIdentifier = command.Email },
                        this.HttpContext.RequestAborted);

                    verificationResult.IfLeft(failure =>
                    {
                        VerificationEmailFailedLogger(this.logger, failure.Message, null);
                        message =
                            "User created, but verification email could not be sent right now. Please retry from login.";
                    });
                }
                catch (Exception ex)
                {
                    VerificationEmailFailedLogger(this.logger, ex.Message, null);
                    message =
                        "User created, but verification email could not be sent right now. Please retry from login.";
                }

                return this.CreatedAtAction(
                    nameof(this.CreateUser),
                    new { id = userId },
                    new { userId, message });
            },
            failure => Task.FromResult<IActionResult>(
                this.BadRequest(new ValidationErrorResponse { Errors = [failure.Message] })));
    }

    /// <summary>
    ///     Returns all profiles associated with the specified user.
    /// </summary>
    /// <param name="userId">User ID from route.</param>
    /// <returns>200 OK with profile list, or 403 when caller is not the same user.</returns>
    [HttpGet("{userId:guid}/profiles")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public Task<IActionResult> GetProfiles([FromRoute] Guid userId)
    {
        if (!this.IsSubjectMatchingUserId(userId))
        {
            return Task.FromResult<IActionResult>(this.Forbid());
        }

        List<UserProfileDto> profiles = this.LoadProfilesForUser(userId);
        return Task.FromResult<IActionResult>(this.Ok(profiles));
    }

    /// <summary>
    ///     Creates a new profile for the authenticated user.
    /// </summary>
    /// <param name="userId">User ID from route.</param>
    /// <param name="command">Profile creation payload.</param>
    /// <returns>201 Created on success, 400 for validation errors, or 403 when caller is not the same user.</returns>
    [HttpPost("{userId:guid}/profiles")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> CreateProfile([FromRoute] Guid userId, [FromBody] UserCreateProfileCommand command)
    {
        if (!this.IsSubjectMatchingUserId(userId))
        {
            return this.Forbid();
        }

        command.UserId = userId;

        List<UserProfileDto> existingProfiles = this.LoadProfilesForUser(userId);
        if (existingProfiles.Any(p =>
                string.Equals(p.ProfileType, command.ProfileType, StringComparison.OrdinalIgnoreCase)))
        {
            return this.BadRequest(
                new ValidationErrorResponse
                {
                    Errors = ["This profile type already exists for the user."]
                });
        }

        Either<Failure, Guid> result = await this.createProfileHandler.Handle(command, this.HttpContext.RequestAborted);

        return result.Match<IActionResult>(
            profileId => this.CreatedAtAction(
                nameof(this.GetProfiles),
                new { userId },
                new { profileId, message = "Profile created." }),
            failure => this.BadRequest(new ValidationErrorResponse { Errors = [failure.Message] }));
    }

    /// <summary>
    ///     Reads a previously uploaded profile image by blob key.
    /// </summary>
    /// <param name="blobKey">Storage blob key.</param>
    /// <returns>Image content when found; otherwise 404 Not Found.</returns>
    [HttpGet("profile-images/{blobKey}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProfileImage([FromRoute] string blobKey)
    {
        (byte[] Content, string ContentType)? image = await this.profileImageStorage.ReadProfileImageAsync(
            blobKey,
            this.HttpContext.RequestAborted);
        if (image == null)
        {
            return this.NotFound();
        }

        return this.File(image.Value.Content, image.Value.ContentType);
    }

    /// <summary>
    ///     Updates profile image for an existing profile.
    /// </summary>
    /// <param name="userId">User ID from route.</param>
    /// <param name="profileId">Profile ID from route.</param>
    /// <param name="command">Image update payload.</param>
    /// <returns>200 OK on success, 400 for validation errors, or 403 when caller is not the same user.</returns>
    [HttpPut("{userId:guid}/profiles/{profileId:guid}/image")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UpdateProfileImage(
        [FromRoute] Guid userId,
        [FromRoute] Guid profileId,
        [FromBody] UserUpdateProfileImageCommand command)
    {
        if (!this.IsSubjectMatchingUserId(userId))
        {
            return this.Forbid();
        }

        command.UserId = userId;
        command.ProfileId = profileId;

        Either<Failure, Unit> result =
            await this.updateProfileImageHandler.Handle(command, this.HttpContext.RequestAborted);

        return result.Match<IActionResult>(
            _ => this.Ok(new { message = "Profile image updated." }),
            failure => this.BadRequest(new ValidationErrorResponse { Errors = [failure.Message] }));
    }

    /// <summary>
    ///     Rehydrates user profile projection from profile-created and profile-image-updated events.
    /// </summary>
    private List<UserProfileDto> LoadProfilesForUser(Guid userId)
    {
        List<UserProfileDto> profiles = [];

        foreach (StoredEvent storedEvent in this.writeContext.StoredEvents
                     .OrderBy(e => e.Timestamp))
        {
            if (storedEvent.EventType.EndsWith("UserProfileCreatedEvent", StringComparison.Ordinal))
            {
                UserProfileCreatedEvent? createdEvent =
                    JsonSerializer.Deserialize<UserProfileCreatedEvent>(storedEvent.Data);
                if (createdEvent == null || createdEvent.UserId != userId)
                {
                    continue;
                }

                profiles.Add(
                    new UserProfileDto
                    {
                        ProfileId = createdEvent.AggregateId,
                        UserId = createdEvent.UserId,
                        ProfileType = createdEvent.PersonType.ToString(),
                        DisplayName = createdEvent.DisplayName,
                        ProfileImageUrl = string.IsNullOrWhiteSpace(createdEvent.ProfileImageBlobKey)
                            ? null
                            : $"{this.Request.Scheme}://{this.Request.Host}/api/v1/users/profile-images/{Uri.EscapeDataString(createdEvent.ProfileImageBlobKey)}"
                    });

                continue;
            }

            if (!storedEvent.EventType.EndsWith("UserProfileImageUpdatedEvent", StringComparison.Ordinal))
            {
                continue;
            }

            UserProfileImageUpdatedEvent? imageUpdatedEvent =
                JsonSerializer.Deserialize<UserProfileImageUpdatedEvent>(storedEvent.Data);
            if (imageUpdatedEvent == null)
            {
                continue;
            }

            UserProfileDto? existing = profiles.FirstOrDefault(p => p.ProfileId == imageUpdatedEvent.AggregateId);
            if (existing == null)
            {
                continue;
            }

            existing.ProfileImageUrl =
                $"{this.Request.Scheme}://{this.Request.Host}/api/v1/users/profile-images/{Uri.EscapeDataString(imageUpdatedEvent.ProfileImageBlobKey)}";
        }

        return profiles;
    }

    /// <summary>
    ///     Checks whether JWT subject claim matches route user ID.
    /// </summary>
    private bool IsSubjectMatchingUserId(Guid userId)
    {
        string? subjectValue = this.User.FindFirstValue(ClaimTypes.NameIdentifier)
                               ?? this.User.FindFirstValue("sub");

        if (!Guid.TryParse(subjectValue, out Guid subjectId))
        {
            return false;
        }

        return subjectId == userId;
    }
}