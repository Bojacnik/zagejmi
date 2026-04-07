using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using LanguageExt;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Zagejmi.Contracts.Commands;
using Zagejmi.Shared.Failures;
using Zagejmi.Shared.Models;
using Zagejmi.Write.Application.Abstractions;

namespace Zagejmi.Write.Api.Controllers;

/// <summary>
///     Handles user management operations.
///     Provides endpoints for user registration and account creation.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : ControllerBase
{
    /// <summary>
    ///     The handler for processing user creation commands.
    /// </summary>
    private readonly IHandlerRequest<UserCreateCommand, Either<Failure, Guid>> createUserHandler;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UsersController" /> class.
    /// </summary>
    /// <param name="createUserHandler">The handler for processing create user commands.</param>
    public UsersController(IHandlerRequest<UserCreateCommand, Either<Failure, Guid>> createUserHandler)
    {
        this.createUserHandler = createUserHandler;
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

        return result.Match<IActionResult>(
            userId => this.CreatedAtAction(nameof(this.CreateUser), new { id = userId }, new { userId }),
            failure => this.BadRequest(new ValidationErrorResponse { Errors = new List<string> { failure.message } }));
    }
}