using System;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Zagejmi.Application.Abstractions;

namespace Zagejmi.Api.Controllers;

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

    private readonly IHandlerRequest<UserCreateProfileCommand, (? Result, Failure? Error)> createProfileHandler;
    private readonly IHandlerRequest<UserCreateServiceCommand, (? Result, Failure? Error)> createServiceHandler;

    /// <summary>
    ///     The handler for processing user creation commands.
    /// </summary>
    private readonly IHandlerRequest<UserCreateCommand, (? Result, Failure? Error)> createUserHandler;

    private readonly ILogger<UsersController> logger;
    private readonly IHandlerRequest<UserRemoveProfileImageCommand, (? Result, Failure? Error)> removeProfileImageHandler;

    private readonly IHandlerRequest<UserSendEmailVerificationCommand, (? Result, Failure? Error)>
        sendEmailVerificationHandler;

    private readonly IHandlerRequest<UserUpdateProfileBannerCommand, (? Result, Failure? Error)> updateProfileBannerHandler;

    private readonly IHandlerRequest<UserUpdateProfileImageCommand, (? Result, Failure? Error)> updateProfileImageHandler;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UsersController" /> class.
    /// </summary>
    /// <param name="createUserHandler">The handler for processing create user commands.</param>
    /// <param name="createProfileHandler">The handler for creating user profiles.</param>
    /// <param name="updateProfileImageHandler">The handler for updating profile images.</param>
    /// <param name="removeProfileImageHandler">Handler for removing profile images.</param>
    /// <param name="updateProfileBannerHandler">Handler for updating profile banners.</param>
    /// <param name="createServiceHandler">Handler for creating services.</param>
    /// <param name="sendEmailVerificationHandler">The handler for sending verification emails.</param>
    /// <param name="logger">The logger instance.</param>
    public UsersController(
        IHandlerRequest<UserCreateCommand, (? Result, Failure? Error)> createUserHandler,
        IHandlerRequest<UserCreateProfileCommand, (? Result, Failure? Error)> createProfileHandler,
        IHandlerRequest<UserUpdateProfileImageCommand, (? Result, Failure? Error)> updateProfileImageHandler,
        IHandlerRequest<UserRemoveProfileImageCommand, (? Result, Failure? Error)> removeProfileImageHandler,
        IHandlerRequest<UserUpdateProfileBannerCommand, (? Result, Failure? Error)> updateProfileBannerHandler,
        IHandlerRequest<UserCreateServiceCommand, (? Result, Failure? Error)> createServiceHandler,
        IHandlerRequest<UserSendEmailVerificationCommand, (? Result, Failure? Error)> sendEmailVerificationHandler,
        ILogger<UsersController> logger)
    {
        this.createUserHandler = createUserHandler;
        this.createProfileHandler = createProfileHandler;
        this.updateProfileImageHandler = updateProfileImageHandler;
        this.removeProfileImageHandler = removeProfileImageHandler;
        this.updateProfileBannerHandler = updateProfileBannerHandler;
        this.createServiceHandler = createServiceHandler;
        this.sendEmailVerificationHandler = sendEmailVerificationHandler;
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
        (? Result, Failure? Error) result = await createUserHandler.Handle(command, HttpContext.RequestAborted);

        return await result.Match<Task<IActionResult>>(
            async userId =>
            {
                string message = "User created. Check your email to verify ownership.";

                try
                {
                    (? Result, Failure? Error) verificationResult = await sendEmailVerificationHandler.Handle(
                        new UserSendEmailVerificationCommand { UserIdentifier = command.Email },
                        HttpContext.RequestAborted);

                    verificationResult.IfLeft(failure =>
                    {
                        VerificationEmailFailedLogger(logger, failure.Message, null);
                        message =
                            "User created, but verification email could not be sent right now. Please retry from login.";
                    });
                }
                catch (Exception ex)
                {
                    VerificationEmailFailedLogger(logger, ex.Message, null);
                    message =
                        "User created, but verification email could not be sent right now. Please retry from login.";
                }

                return CreatedAtAction(
                    nameof(CreateUser),
                    new { id = userId },
                    new { userId, message });
            },
            failure => Task.FromResult<IActionResult>(
                this.BadRequest(new ValidationErrorResponse { Errors = new() {failure.Message] })));
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
        if (!IsSubjectMatchingUserId(userId)) return Forbid();

        command.UserId = userId;

        (? Result, Failure? Error) result = await createProfileHandler.Handle(command, HttpContext.RequestAborted);

        return result.Match<IActionResult>(
            profileId => CreatedAtAction(
                nameof(CreateProfile),
                new { userId, profileId },
                new { profileId, message = "Profile created." }),
            failure => this.BadRequest(new ValidationErrorResponse { Errors = new() {failure.Message] }));
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
        if (!IsSubjectMatchingUserId(userId)) return Forbid();

        command.UserId = userId;
        command.ProfileId = profileId;

        (? Result, Failure? Error) result =
            await updateProfileImageHandler.Handle(command, HttpContext.RequestAborted);

        return result.Match<IActionResult>(
            _ => Ok(new { message = "Profile image updated." }),
            failure => this.BadRequest(new ValidationErrorResponse { Errors = new() {failure.Message] }));
    }

    /// <summary>
    ///     Updates the banner image for an associate profile.
    /// </summary>
    [HttpPut("{userId:guid}/profiles/{profileId:guid}/banner")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UpdateProfileBanner([
            FromRoute]
        Guid userId,
        [FromRoute] Guid profileId,
        [FromBody] UserUpdateProfileBannerCommand command)
    {
        if (!IsSubjectMatchingUserId(userId)) return Forbid();

        command.UserId = userId;
        command.ProfileId = profileId;

        (? Result, Failure? Error) result = await updateProfileBannerHandler.Handle(command, HttpContext.RequestAborted);

        return result.Match<IActionResult>(_ => Ok(new { message = "Banner updated." }),
            failure => this.BadRequest(new ValidationErrorResponse { Errors = new() {failure.Message] }));
    }

    /// <summary>
    ///     Creates a new service offering for an associate profile.
    /// </summary>
    [HttpPost("{userId:guid}/profiles/{profileId:guid}/services")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> CreateService([
            FromRoute]
        Guid userId,
        [FromRoute] Guid profileId,
        [FromBody] UserCreateServiceCommand command)
    {
        if (!IsSubjectMatchingUserId(userId)) return Forbid();

        command.UserId = userId;
        command.ProfileId = profileId;

        (? Result, Failure? Error) result = await createServiceHandler.Handle(command, HttpContext.RequestAborted);

        return result.Match<IActionResult>(serviceId => CreatedAtAction(
                nameof(CreateService),
                new { userId, profileId, serviceId },
                new { serviceId, message = "Service created." }),
            failure => this.BadRequest(new ValidationErrorResponse { Errors = new() {failure.Message] }));
    }

    /// <summary>
    ///     Removes the profile image (soft delete) for the specified profile.
    /// </summary>
    [HttpDelete("{userId:guid}/profiles/{profileId:guid}/image")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> RemoveProfileImage(
        [
            FromRoute]
        Guid userId,
        [FromRoute] Guid profileId)
    {
        if (!IsSubjectMatchingUserId(userId)) return Forbid();

        UserRemoveProfileImageCommand command = new() { UserId = userId, ProfileId = profileId };
        (? Result, Failure? Error) result =
            await removeProfileImageHandler.Handle(command, HttpContext.RequestAborted);

        return result.Match<IActionResult>(
            _ => Ok(new { message = "Profile image removed." }),
            failure => this.BadRequest(new ValidationErrorResponse { Errors = new() {failure.Message] }));
    }

    /// <summary>
    ///     Checks whether JWT subject claim matches route user ID.
    /// </summary>
    private bool IsSubjectMatchingUserId(Guid userId)
    {
        string? subjectValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                               ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(subjectValue, out Guid subjectId)) return false;

        return subjectId == userId;
    }
}